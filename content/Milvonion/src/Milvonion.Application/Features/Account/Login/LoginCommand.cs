using Milvasoft.Components.CQRS.Command;
using Milvonion.Application.Dtos.AccountDtos;

namespace Milvonion.Application.Features.Account.Login;

/// <summary>
/// Data transfer object for login operation.
/// </summary>
public record LoginCommand : ICommand<LoginResponseDto>
{
    /// <summary>
    /// Username to log in.
    /// </summary>
    public string UserName { get; set; }

    /// <summary>
    /// Password to log in.
    /// </summary>
    public string Password { get; set; }

    /// <summary>
    /// Unique Id of the device being logged in. Can be MacAddress or something.
    /// </summary>
    public string DeviceId { get; set; }
}