using Milvasoft.Attributes.Annotations;
using System.Linq.Expressions;
using System.Text.Json.Serialization;

namespace Milvonion.Application.Dtos.UserDtos;

/// <summary>
/// Data transfer object for user list.
/// </summary>
[Translate]
public class UserListDto : MilvonionBaseDto<int>
{
    /// <summary>
    /// Unique username of user.
    /// </summary>
    public string UserName { get; set; }

    /// <summary>
    /// Email of user.
    /// </summary>
    [Filterable(false)]
    public string Email { get; set; }

    /// <summary>
    /// Name of user.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Surname of user.
    /// </summary>
    [Filterable(false)]
    public string Surname { get; set; }

    /// <summary>
    /// Type of the user.
    /// </summary>
    public UserType UserType { get; set; }

    /// <summary>
    /// End of the user's lockout (UTC). Null when the user was never locked or the lock was removed.
    /// </summary>
    public DateTimeOffset? LockoutEnd { get; set; }

    /// <summary>
    /// Whether the user is locked out now (<see cref="LockoutEnd"/> is in the future). Computed, not filterable.
    /// </summary>
    [Filterable(false)]
    public bool IsLocked { get; set; }

    /// <summary>
    /// Projection expression for mapping User entity to UserListDto.
    /// </summary>
    [JsonIgnore]
    [ExcludeFromMetadata]
    public static Expression<Func<User, UserListDto>> Projection { get; } = u => new UserListDto
    {
        Id = u.Id,
        UserName = u.UserName,
        Email = u.Email,
        Name = u.Name,
        Surname = u.Surname,
        UserType = u.UserType,
        LockoutEnd = u.LockoutEnd,

        // Same rule as the login (IMilvaUserManager.IsLockedOut): lockout enabled and the end not passed yet.
        IsLocked = u.LockoutEnabled && u.LockoutEnd != null && u.LockoutEnd >= DateTimeOffset.UtcNow
    };
}