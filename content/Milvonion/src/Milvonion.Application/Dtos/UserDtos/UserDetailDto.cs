using Milvasoft.Attributes.Annotations;
using System.Linq.Expressions;
using System.Text.Json.Serialization;

namespace Milvonion.Application.Dtos.UserDtos;

/// <summary>
/// Data transfer object for user details.
/// </summary>
[Translate]
[ExcludeFromMetadata]
public class UserDetailDto : MilvonionBaseDto<int>
{
    /// <summary>
    /// Unique username of the user.
    /// </summary>
    public string UserName { get; set; }

    /// <summary>
    /// Email of the user.
    /// </summary>
    public string Email { get; set; }

    /// <summary>
    /// Name of the user.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Surname of the user.
    /// </summary>
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
    /// Whether the user is locked out now (<see cref="LockoutEnd"/> is in the future).
    /// </summary>
    public bool IsLocked { get; set; }

    /// <summary>
    /// Information about the roles the user is assigned to.
    /// </summary>
    public List<NameIntNavigationDto> Roles { get; set; }

    /// <summary>
    /// Allowed notification types for the user.
    /// </summary>
    public List<NotificationType> AllowedNotifications { get; set; }

    /// <summary>
    /// Information about record audit.
    /// </summary>
    public AuditDto<int> AuditInfo { get; set; }

    /// <summary>
    /// Projection expression for mapping User entity to UserDetailDto.
    /// </summary>
    [JsonIgnore]
    [ExcludeFromMetadata]
    public static Expression<Func<User, UserDetailDto>> Projection { get; } = u => new UserDetailDto
    {
        Id = u.Id,
        UserName = u.UserName,
        Email = u.Email,
        Name = u.Name,
        Surname = u.Surname,
        UserType = u.UserType,
        LockoutEnd = u.LockoutEnd,

        // Same rule as the login (IMilvaUserManager.IsLockedOut): lockout enabled and the end not passed yet.
        IsLocked = u.LockoutEnabled && u.LockoutEnd != null && u.LockoutEnd >= DateTimeOffset.UtcNow,
        Roles = u.RoleRelations.Select(rr => new NameIntNavigationDto { Id = rr.Role.Id, Name = rr.Role.Name }).ToList(),
        AllowedNotifications = u.AllowedNotifications,
        AuditInfo = new AuditDto<int>(u)
    };
}
