using Microsoft.AspNetCore.Http;
using Milvasoft.Components.CQRS.Command;
using Milvasoft.Components.Rest.MilvaResponse;
using Milvasoft.Core.Abstractions;
using Milvasoft.Interception.Ef.Transaction;
using Milvonion.Application.Dtos.AccountDtos;

namespace Milvonion.Application.Features.Account.RefreshLogin;

/// <summary>
/// Handles the RefreshLoginCommand and refreshes the user's login token. The refresh token is consumed
/// atomically (a single conditional delete of its session row), so two concurrent refreshes with the same
/// token cannot both succeed.
/// </summary>
/// <param name="UserRepository"></param>
/// <param name="UserSessionRepository"></param>
/// <param name="UserSessionHistoriesRepository"></param>
/// <param name="AccountManager"></param>
/// <param name="HttpContextAccessor"></param>
[Transaction]
public record RefreshLoginCommandHandler(IMilvonionRepositoryBase<User> UserRepository,
                                         IMilvonionRepositoryBase<UserSession> UserSessionRepository,
                                         IMilvonionRepositoryBase<UserSessionHistory> UserSessionHistoriesRepository,
                                         IAccountManager AccountManager,
                                         IHttpContextAccessor HttpContextAccessor) : IInterceptable, ICommandHandler<RefreshLoginCommand, LoginResponseDto>
{
    private readonly IMilvonionRepositoryBase<User> _userRepository = UserRepository;
    private readonly IMilvonionRepositoryBase<UserSession> _userSessionRepository = UserSessionRepository;
    private readonly IMilvonionRepositoryBase<UserSessionHistory> _userSessionHistoriesRepository = UserSessionHistoriesRepository;
    private readonly IAccountManager _accountManager = AccountManager;
    private readonly IHttpContextAccessor _httpContextAccessor = HttpContextAccessor;

    /// <inheritdoc/>
    public async Task<Response<LoginResponseDto>> Handle(RefreshLoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetFirstOrDefaultAsync(i => i.UserName == request.UserName, User.Projections.Login, cancellationToken: cancellationToken);

        if (user == null)
            return Response<LoginResponseDto>.Error(null, MessageKey.Unauthorized);

        var relatedSession = user.Sessions.Find(s => s.RefreshToken == request.RefreshToken && s.DeviceId == request.DeviceId);

        var currentToken = _httpContextAccessor.GetTokenFromHeader();

        if (relatedSession == null || relatedSession.AccessToken != currentToken)
            return Response<LoginResponseDto>.Error(null, MessageKey.Unauthorized);

        if (relatedSession.ExpiryDate < DateTime.UtcNow)
            return Response<LoginResponseDto>.Error(null, MessageKey.SessionTimeout);

        // Consume the refresh token atomically: whoever deletes the row wins; a replay finds nothing.
        var consumed = await _userSessionRepository.ExecuteDeleteAsync(s => s.Id == relatedSession.Id && s.RefreshToken == request.RefreshToken && s.DeviceId == request.DeviceId,
                                                                       cancellationToken: cancellationToken);

        if (consumed == 0)
            return Response<LoginResponseDto>.Error(null, MessageKey.Unauthorized);

        // The row is gone; archive it like the login archives a replaced session, and keep the in-memory session list in
        // sync so the login does not try to remove it again.
        await _userSessionHistoriesRepository.AddAsync(new UserSessionHistory(relatedSession), cancellationToken);

        user.Sessions.Remove(relatedSession);

        var tokenModel = await _accountManager.LoginAsync(user, request.DeviceId, cancellationToken);

        var loginResponse = new LoginResponseDto
        {
            Id = user.Id,
            UserType = user.UserType,
            Token = tokenModel
        };

        return Response<LoginResponseDto>.Success(loginResponse);
    }
}
