using Milvasoft.Core.Helpers;
using Milvasoft.Identity.Abstract;
using Milvasoft.Identity.Concrete;
using Milvasoft.Identity.Concrete.Options;
using Milvonion.Application.Interfaces;
using Milvonion.Application.Utils.Constants;
using Milvonion.Domain;
using System.Security.Claims;

namespace Milvonion.Infrastructure.Services;

/// <summary>
/// User login operation.
/// It updates sessions in database.
/// Generates access and refresh tokens.
/// </summary>
/// <returns></returns>
public class AccountManager(IMilvonionRepositoryBase<UserSession> userSessionRepository,
                            IMilvaTokenManager milvaTokenManager,
                            MilvaIdentityOptions identityOptions) : IAccountManager
{
    private readonly IMilvonionRepositoryBase<UserSession> _userSessionRepository = userSessionRepository;
    private readonly IMilvaTokenManager _milvaTokenManager = milvaTokenManager;
    private readonly MilvaIdentityOptions _identityOptions = identityOptions;

    /// <summary>
    /// User login operation.
    /// It updates sessions in database.
    /// Generates access and refresh tokens.
    /// </summary>
    /// <returns></returns>
    public async Task<MilvaToken> LoginAsync(User user, string deviceId, CancellationToken cancellationToken = default)
    {
        var permissions = user.RoleRelations.SelectMany(i => i.Role.RolePermissionRelations.Select(i => i.Permission)).ToList();

        List<Claim> claims = [new(ClaimTypes.Name, user.UserName)];

        claims.AddRange(BuildRoleClaims(permissions));
        claims.Add(BuildUserTypeClaim(user));

        var accessToken = _milvaTokenManager.GenerateToken(null, claims: [.. claims]);

        var milvaToken = new MilvaToken
        {
            AccessToken = accessToken,
            ExpiresIn = _identityOptions.Token.ExpirationMinute * 60,
            Scope = "Milvonion",
            TokenType = "Bearer",
            RefreshToken = IdentityHelpers.CreateRefreshToken(),
            RefreshTokenExpiresIn = _identityOptions.Token.ExpirationMinute * 600,
        };

        var sessionsToRemove = FindSessionsToRemove(user.Sessions, deviceId);

        if (sessionsToRemove.Count > 0)
            await _userSessionRepository.BulkDeleteAsync(sessionsToRemove, cancellationToken: cancellationToken);

        var newSession = new UserSession
        {
            UserName = user.UserName,
            UserId = user.Id,
            AccessToken = milvaToken.AccessToken,
            RefreshToken = milvaToken.RefreshToken,
            ExpiryDate = DateTime.UtcNow.AddSeconds(milvaToken.RefreshTokenExpiresIn),
            DeviceId = deviceId,
        };

        await _userSessionRepository.AddAsync(newSession, cancellationToken: cancellationToken);

        return milvaToken;
    }

    /// <summary>
    /// Generates token for ci user.
    /// Generates access token.
    /// </summary>
    /// <returns></returns>
    public string GenerateToken(User user, DateTime? expiryDate = null)
    {
        var permissions = user.RoleRelations.SelectMany(i => i.Role.RolePermissionRelations.Select(i => i.Permission)).ToList();

        List<Claim> claims = [new(ClaimTypes.Name, user.UserName)];

        claims.AddRange(BuildRoleClaims(permissions));
        claims.Add(BuildUserTypeClaim(user));

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
        List<UserSession> sessionsToRemove = userSessions.Where(s => s.DeviceId == deviceId || s.ExpiryDate < DateTime.UtcNow).ToList();

        if (userSessions.Count - sessionsToRemove.Count > 4)
            sessionsToRemove.AddRange(userSessions.OrderBy(s => s.CreationDate).Take(1));

        return sessionsToRemove;
    }

    private static IEnumerable<Claim> BuildRoleClaims(List<Permission> permissions) => permissions?.Select(p => new Claim(ClaimTypes.Role, p.FormatPermissionAndGroup())) ?? [];

    private static Claim BuildUserTypeClaim(User user) => new(GlobalConstant.UserTypeClaimName, user.UserType.ToString());
}
