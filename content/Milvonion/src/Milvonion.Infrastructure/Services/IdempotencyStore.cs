using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Milvasoft.DataAccess.EfCore.Bulk;
using Milvonion.Application.Interfaces;
using Milvonion.Application.Utils.Models;
using Milvonion.Domain;
using Npgsql;
using NpgsqlTypes;
using System.Text;

namespace Milvonion.Infrastructure.Services;

/// <summary>
/// Table-backed idempotency keys (see <see cref="IIdempotencyStore"/>). The claim is a single
/// <c>INSERT ... ON CONFLICT DO NOTHING</c>, so the happy path of a create endpoint costs one extra
/// statement; only a retry pays for the read that follows.
/// </summary>
/// <remarks>
/// A claim whose request never finished (the process died between the claim and the response) is taken over
/// after <see cref="PendingTimeout"/>, otherwise that key would stay blocked until the cleanup job runs. The
/// window is far longer than any of the guarded endpoints takes, so a still-running request is never taken over.
/// </remarks>
/// <param name="dbContextAccessor"></param>
/// <param name="logger"></param>
public class IdempotencyStore(IMilvonionDbContextAccessor dbContextAccessor, ILogger<IdempotencyStore> logger) : IIdempotencyStore
{
    private readonly IMilvaBulkDbContextBase _dbContext = dbContextAccessor.GetDbContext();
    private readonly ILogger<IdempotencyStore> _logger = logger;

    /// <summary>A claim older than this with no response is considered abandoned and can be taken over.</summary>
    public static readonly TimeSpan PendingTimeout = TimeSpan.FromSeconds(60);

    /// <summary>Responses larger than this are not stored; a retry of such a request is answered with "in progress".</summary>
    public const int MaxResponseBytes = 16 * 1024;

    private const string _claimSql = """
        INSERT INTO "IdempotencyKeys" ("UserId", "Key", "Endpoint", "RequestHash", "CreationDate")
        VALUES ({0}, {1}, {2}, {3}, now())
        ON CONFLICT ("UserId", "Key") DO NOTHING
        """;

    private const string _takeOverSql = """
        UPDATE "IdempotencyKeys"
           SET "Endpoint" = {2}, "RequestHash" = {3}, "ResponseData" = NULL, "CreationDate" = now()
         WHERE "UserId" = {0}
           AND "Key" = {1}
           AND "CompletedAt" IS NULL
           AND "CreationDate" < now() - {4}::interval
        """;

    private const string _completeSql = """
        UPDATE "IdempotencyKeys"
           SET "ResponseData" = {2}, "CompletedAt" = now()
         WHERE "UserId" = {0}
           AND "Key" = {1}
           AND "CompletedAt" IS NULL
        """;

    private const string _releaseSql = """
        DELETE FROM "IdempotencyKeys"
         WHERE "UserId" = {0}
           AND "Key" = {1}
           AND "CompletedAt" IS NULL
        """;

    /// <inheritdoc/>
    public async Task<IdempotencyClaim> ClaimAsync(int userId, string key, string endpoint, byte[] requestHash, CancellationToken cancellationToken)
    {
        var inserted = await _dbContext.Database.ExecuteSqlRawAsync(_claimSql, [userId, key, endpoint, HashParameter(requestHash)], cancellationToken);

        if (inserted == 1)
            return new IdempotencyClaim { Claimed = true };

        // Someone claimed this key before: either the first attempt is still running or it already answered.
        var stored = await _dbContext.Set<IdempotencyKey>()
                                     .AsNoTracking()
                                     .Where(k => k.UserId == userId && k.Key == key)
                                     .Select(k => new IdempotencyKey
                                     {
                                         RequestHash = k.RequestHash,
                                         ResponseData = k.ResponseData,
                                         CompletedAt = k.CompletedAt,
                                         CreationDate = k.CreationDate,
                                     })
                                     .FirstOrDefaultAsync(cancellationToken);

        // The row was pruned between the two statements; let the request through rather than blocking it.
        if (stored == null)
            return new IdempotencyClaim { Claimed = true };

        if (!SameRequest(stored.RequestHash, requestHash))
            return new IdempotencyClaim { RequestMismatch = true };

        if (stored.CompletedAt.HasValue)
            return new IdempotencyClaim { ResponseData = stored.ResponseData };

        var takenOver = await _dbContext.Database.ExecuteSqlRawAsync(_takeOverSql, [userId, key, endpoint, HashParameter(requestHash), $"{PendingTimeout.TotalSeconds} seconds"], cancellationToken);

        if (takenOver == 1)
        {
            _logger.LogWarning("Took over an abandoned idempotency claim on {Endpoint}.", endpoint);

            return new IdempotencyClaim { Claimed = true };
        }

        return new IdempotencyClaim();
    }

    /// <inheritdoc/>
    public async Task CompleteAsync(int userId, string key, string responseData, CancellationToken cancellationToken)
    {
        if (responseData != null && Encoding.UTF8.GetByteCount(responseData) > MaxResponseBytes)
        {
            _logger.LogWarning("Idempotent response is larger than {MaxResponseBytes} bytes; it is not stored for replay.", MaxResponseBytes);

            return;
        }

        await _dbContext.Database.ExecuteSqlRawAsync(_completeSql, [userId, key, responseData], cancellationToken);
    }

    /// <inheritdoc/>
    public Task ReleaseAsync(int userId, string key, CancellationToken cancellationToken)
        => _dbContext.Database.ExecuteSqlRawAsync(_releaseSql, [userId, key], cancellationToken);

    // bytea has no implicit text form, so the hash is bound as an explicitly typed parameter.
    private static NpgsqlParameter HashParameter(byte[] requestHash) => new() { NpgsqlDbType = NpgsqlDbType.Bytea, Value = requestHash };

    private static bool SameRequest(byte[] stored, byte[] current) => stored != null && current != null && stored.AsSpan().SequenceEqual(current);
}
