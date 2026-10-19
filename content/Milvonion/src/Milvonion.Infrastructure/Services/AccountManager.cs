using Microsoft.AspNetCore.Http;
using Milvasoft.Core.Helpers;
using Milvasoft.Identity.Abstract;
using Milvasoft.Identity.Concrete;
using Milvasoft.Identity.Concrete.Options;
using Milvonion.Application.Interfaces;
using Milvonion.Application.Utils.Constants;
using Milvonion.Application.Utils.Extensions;
using Milvonion.Domain;
using System.Security.Claims;

namespace Milvonion.Infrastructure.Services;

/// <summary>
/// User login operation.
/// It updates sessions in database.
/// Generates access and refresh tokens.
/// </summary>
/// <param name="userSessionRepository"></param>
/// <param name="userSessionHistoriesRepository"></param>
/// <param name="milvaTokenManager"></param>
/// <param name="identityOptions"></param>
/// <param name="securityStampStore"></param>
/// <param name="httpContextAccessor"></param>
/// <returns></returns>
public class AccountManager(IMilvonionRepositoryBase<UserSession> userSessionRepository,
                            IMilvonionRepositoryBase<UserSessionHistory> userSessionHistoriesRepository,
                            IMilvaTokenManager milvaTokenManager,
                            MilvaIdentityOptions identityOptions,
                            ISecurityStampStore securityStampStore,
                            IHttpContextAccessor httpContextAccessor) : IAccountManager
{
    private readonly IMilvonionRepositoryBase<UserSession> _userSessionRepository = userSessionRepository;
    private readonly IMilvonionRepositoryBase<UserSessionHistory> _userSessionHistoriesRepository = userSessionHistoriesRepository;
    private readonly IMilvaTokenManager _milvaTokenManager = milvaTokenManager;
    private readonly MilvaIdentityOptions _identityOptions = identityOptions;
    private readonly ISecurityStampStore _securityStampStore = securityStampStore;
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    /// <summary>
    /// User login operation.
    /// It updates sessions in database.
    /// Generates access and refresh tokens.
    /// </summary>
    /// <returns></returns>
    public async Task<MilvaToken> LoginAsync(User user, string deviceId, CancellationToken cancellationToken = default)
    {
        var claims = await BuildClaimsAsync(user, cancellationToken);

        var accessToken = _milvaTokenManager.GenerateToken(null, claims: [.. claims]);

        var milvaToken = new MilvaToken
        {
            AccessToken = accessToken,
            ExpiresIn = _identityOptions.Token.ExpirationMinute * 60,
            Scope = "Milvonion",
            TokenType = "Bearer",
            RefreshToken = IdentityHelpers.CreateRefreshToken(),
            RefreshTokenExpiresIn = _identityOptions.Token.ExpirationMinute * 2880 // seconds: 48x the access token lifetime (24 hours with a 30-minute token, 72 hours with 90)
        };

        var sessionsToRemove = FindSessionsToRemove(user.Sessions, deviceId);

        if (sessionsToRemove.Count > 0)
        {
            await _userSessionRepository.BulkDeleteAsync(sessionsToRemove, cancellationToken: cancellationToken);

            var sessionsToArchive = sessionsToRemove.Select(s => new UserSessionHistory(s)).ToList();

            await _userSessionHistoriesRepository.BulkAddAsync(sessionsToArchive, cancellationToken: cancellationToken);
        }

        var newSession = new UserSession
        {
            UserName = user.UserName,
            UserId = user.Id,
            AccessToken = milvaToken.AccessToken,
            RefreshToken = milvaToken.RefreshToken,
            ExpiryDate = DateTime.UtcNow.AddSeconds(milvaToken.RefreshTokenExpiresIn),
            DeviceId = deviceId,
            IpAddress = _httpContextAccessor.HttpContext.GetIpAddress()
        };

        await _userSessionRepository.AddAsync(newSession, cancellationToken: cancellationToken);

        return milvaToken;
    }

    /// <summary>
    /// Generates token for ci user.
    /// Generates access token carrying the user's current revocation stamp.
    /// </summary>
    /// <returns></returns>
    public async Task<string> GenerateTokenAsync(User user, DateTime? expiryDate = null, CancellationToken cancellationToken = default)
    {
        // Long-lived CI tokens carry the current stamp too: the revocation middleware rejects any token without
        // one, and this keeps them revocable by rotating the stamp.
        var claims = await BuildClaimsAsync(user, cancellationToken);

        expiryDate ??= DateTime.UtcNow.AddYears(20);

        var accessToken = _milvaTokenManager.GenerateToken(expiryDate, claims: [.. claims]);

        return accessToken;
    }

    /// <summary>
    /// Finds the sessions to remove. Find criteria is the device id and the expiry date.
    /// </summary>
    /// <param name="userSessions"></param>
    /// <param name="deviceId"></param>
    /// <returns></returns>
    private static List<UserSession> FindSessionsToRemove(List<UserSession> userSessions, string deviceId)
    {
        if (userSessions.IsNullOrEmpty())
            return [];

        // According to expired date and duplicate device id, find the sessions to remove.
        List<UserSession> sessionsToRemove = [.. userSessions.Where(s => s.DeviceId == deviceId || s.ExpiryDate < DateTime.UtcNow)];

        if (userSessions.Count - sessionsToRemove.Count > 4)
            sessionsToRemove.AddRange(userSessions.OrderBy(s => s.CreationDate).Take(1));

        return sessionsToRemove;
    }

    /// <summary>
    /// Builds the claims of an access token: user name, user id, permissions (as role claims), user type and the
    /// revocation stamp. The stamp is assigned on first use and checked on every request (SecurityStampValidationMiddleware).
    /// </summary>
    /// <param name="user"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task<List<Claim>> BuildClaimsAsync(User user, CancellationToken cancellationToken)
    {
        var permissions = user.RoleRelations.SelectMany(i => i.Role.RolePermissionRelations.Select(i => i.Permission));

        List<Claim> claims =
        [
            new(ClaimTypes.Name, user.UserName),
            new(GlobalConstant.UserIdClaimName, user.Id.ToString())
        ];

        claims.AddRange(BuildRoleClaims(permissions));
        claims.Add(BuildUserTypeClaim(user));

        var sessionStamp = await _securityStampStore.GetOrCreateAsync(user.Id, cancellationToken);

        claims.Add(new(GlobalConstant.SessionStampClaimName, sessionStamp));

        return claims;
    }

    private static IEnumerable<Claim> BuildRoleClaims(IEnumerable<Permission> permissions) => permissions?.Select(p => new Claim(ClaimTypes.Role, p.FormatPermissionAndGroup())) ?? [];

    private static Claim BuildUserTypeClaim(User user) => new(GlobalConstant.UserTypeClaimName, user.UserType.ToString());
}
