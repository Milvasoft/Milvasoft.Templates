using Milvasoft.Components.CQRS.Command;
using Milvasoft.Interception.Interceptors.Logging;

namespace Milvonion.Application.Features.Account.ChangePassword;

/// <summary>
/// Data transfer object for password change operation.
/// </summary>
public record ChangePasswordCommand : ICommand
{
    /// <summary>
    /// Username whose password will be changed
    /// </summary>
    public string UserName { get; set; }

    /// <summary>
    /// Old password of account.
    /// </summary>
    [LogMasked]
    public string OldPassword { get; set; }

    /// <summary>
    /// New password.
    /// </summary>
    [LogMasked]
    public string NewPassword { get; set; }
}
