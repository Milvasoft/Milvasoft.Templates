using Milvasoft.Components.CQRS.Query;
using Milvonion.Application.Dtos.UserDtos;

namespace Milvonion.Application.Features.Users.GetUserDetail;

/// <summary>
/// Data transfer object for user details.
/// </summary>
public record GetUserDetailQuery : IQuery<UserDetailDto>
{
    /// <summary>
    /// Id of the user whose details will be accessed.
    /// </summary>
    public int UserId { get; set; }
}
