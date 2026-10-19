using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Milvasoft.Attributes.Annotations;
using Milvasoft.Core.EntityBases.Abstract;
using Milvasoft.Identity.Concrete.Entity;
using Milvonion.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq.Expressions;

namespace Milvonion.Domain;

/// <summary>
/// Entity of the Users table.
/// </summary>
[Table(TableNames.Users)]
[Index(nameof(UserName), nameof(IsDeleted), nameof(DeletionDate), IsUnique = true)]
// The composite index above does not enforce uniqueness among active users: under Postgres' default NULLS DISTINCT
// their NULL DeletionDate makes every row distinct. The plain unique index on the normalized user name both serves
// the login/create lookups and enforces uniqueness at the database level, matching the application, which rejects a
// name already present on ANY row (soft-deleted included). The e-mail is unique too, but only when it is set: it is
// optional (the root user has none), so that index is filtered and configured in MilvonionDbContext.OnModelCreating.
[Index(nameof(NormalizedUserName), IsUnique = true)]
[DontIndexCreationDate]
public class User : MilvaUser<int>, IFullAuditable<int>
{
    /// <summary>
    /// Column length of <see cref="Name"/> and <see cref="Surname"/>; the create and update validators use the same limit.
    /// </summary>
    public const int NameMaxLength = 100;

    /// <summary>
    /// First name of the user.
    /// </summary>
    [MaxLength(NameMaxLength)]
    public string Name { get; set; }

    /// <summary>
    /// Last name of the user.
    /// </summary>
    [MaxLength(NameMaxLength)]
    public string Surname { get; set; }

    /// <summary>
    /// Type of the user.
    /// </summary>
    public UserType UserType { get; set; } = UserType.AppUser;

    /// <summary>
    /// Allowed notification types for this user.
    /// </summary>
    [Column(TypeName = "jsonb")]
    public List<NotificationType> AllowedNotifications { get; set; } = [];

    /// <summary>
    /// Revocation stamp. Baked into the access token at login; every request compares the token's
    /// stamp against the current value (cached in Redis, the database is the source of truth). Rotating it
    /// (password change, role change, lock, deletion) instantly invalidates every existing access
    /// token of this user. Null until the user's first token is minted (assigned lazily by the security stamp store).
    /// </summary>
    [MaxLength(64)]
    public string SessionStamp { get; set; }

    #region Auditing

    /// <inheritdoc/>
    public DateTime? LastModificationDate { get; set; }

    /// <inheritdoc/>
    public DateTime? CreationDate { get; set; }

    /// <inheritdoc/>
    public string CreatorUserName { get; set; }

    /// <inheritdoc/>
    public string LastModifierUserName { get; set; }

    /// <inheritdoc/>
    public string DeleterUserName { get; set; }

    /// <inheritdoc/>
    public DateTime? DeletionDate { get; set; }

    /// <inheritdoc/>
    public bool IsDeleted { get; set; }

    #endregion

    /// <summary>
    /// Navigation property of roles relation.
    /// </summary>
    [CascadeOnDelete]
    public virtual List<UserRoleRelation> RoleRelations { get; set; }

    /// <summary>
    /// Navigation property of user sessions relation.
    /// </summary>
    [CascadeOnDelete]
    public virtual List<UserSession> Sessions { get; set; }

    /// <summary>
    /// Get current user delegate. Gets the current user from the http context.
    /// </summary>
    /// <param name="serviceProvider"></param>
    /// <returns></returns>
    public static string GetCurrentUser(IServiceProvider serviceProvider)
    {
        var httpContextAccessor = serviceProvider.GetRequiredService<IHttpContextAccessor>();

        return httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "Anonymous";
    }

    #region Projections

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public static class Projections
    {
        public static Expression<Func<User, User>> UserRemove { get; } = u => new User
        {
            Id = u.Id,
            UserName = u.UserName,
            AccessFailedCount = u.AccessFailedCount,
            Email = u.Email,
            CreationDate = u.CreationDate,
            CreatorUserName = u.CreatorUserName,
            LastModificationDate = u.LastModificationDate,
            LastModifierUserName = u.LastModifierUserName,
            DeletionDate = u.DeletionDate,
            DeleterUserName = u.DeleterUserName,
            Name = u.Name,
            Surname = u.Surname,
            UserType = u.UserType,
            EmailConfirmed = u.EmailConfirmed,
            PhoneNumber = u.PhoneNumber,
            PhoneNumberConfirmed = u.PhoneNumberConfirmed,
            Sessions = u.Sessions,
            LockoutEnabled = u.LockoutEnabled,
            LockoutEnd = u.LockoutEnd,
            NormalizedEmail = u.NormalizedEmail,
            NormalizedUserName = u.NormalizedUserName,
            PasswordHash = u.PasswordHash,
            TwoFactorEnabled = u.TwoFactorEnabled,
            RoleRelations = u.RoleRelations,
            IsDeleted = u.IsDeleted,
        };

        public static Expression<Func<User, User>> GenerateToken { get; } = u => new User
        {
            Id = u.Id,
            UserName = u.UserName,
            UserType = u.UserType,
            RoleRelations = u.RoleRelations.Select(r => new UserRoleRelation
            {
                Id = r.Id,
                Role = new Role
                {
                    Id = r.Role.Id,
                    Name = r.Role.Name,
                    RolePermissionRelations = r.Role.RolePermissionRelations.Select(rp => new RolePermissionRelation
                    {
                        Id = rp.Id,
                        PermissionId = rp.PermissionId,
                        RoleId = rp.RoleId,
                        Permission = new Permission
                        {
                            Id = rp.Permission.Id,
                            Name = rp.Permission.Name,
                            PermissionGroup = rp.Permission.PermissionGroup
                        }
                    }).ToList()
                },
                UserId = r.UserId,
                RoleId = r.RoleId

            }).ToList(),
            Sessions = u.Sessions.Select(s => new UserSession
            {
                Id = s.Id,
                AccessToken = s.AccessToken,
                RefreshToken = s.RefreshToken,
                UserId = s.UserId,
                DeviceId = s.DeviceId,
                CreationDate = s.CreationDate,
                IpAddress = s.IpAddress,
                ExpiryDate = s.ExpiryDate,
            }).ToList(),
            IsDeleted = u.IsDeleted,
        };

        public static Expression<Func<User, User>> Login { get; } = u => new User
        {
            Id = u.Id,
            UserName = u.UserName,
            PasswordHash = u.PasswordHash,
            UserType = u.UserType,
            AccessFailedCount = u.AccessFailedCount,
            LockoutEnabled = u.LockoutEnabled,
            LockoutEnd = u.LockoutEnd,
            Sessions = u.Sessions.Select(s => new UserSession
            {
                Id = s.Id,
                AccessToken = s.AccessToken,
                RefreshToken = s.RefreshToken,
                UserId = s.UserId,
                DeviceId = s.DeviceId,
                CreationDate = s.CreationDate,
                IpAddress = s.IpAddress,
                ExpiryDate = s.ExpiryDate,
            }).ToList(),
            RoleRelations = u.RoleRelations.Select(r => new UserRoleRelation
            {
                Id = r.Id,
                Role = new Role
                {
                    Id = r.Role.Id,
                    Name = r.Role.Name,
                    RolePermissionRelations = r.Role.RolePermissionRelations.Select(rp => new RolePermissionRelation
                    {
                        Id = rp.Id,
                        PermissionId = rp.PermissionId,
                        RoleId = rp.RoleId,
                        Permission = new Permission
                        {
                            Id = rp.Permission.Id,
                            Name = rp.Permission.Name,
                            PermissionGroup = rp.Permission.PermissionGroup
                        }
                    }).ToList()
                },
                UserId = r.UserId,
                RoleId = r.RoleId

            }).ToList(),
            IsDeleted = u.IsDeleted,
        };

        public static Expression<Func<User, User>> Permissions { get; } = u => new User
        {
            Id = u.Id,
            UserName = u.UserName,
            RoleRelations = u.RoleRelations.Select(r => new UserRoleRelation
            {
                Id = r.Id,
                Role = new Role
                {
                    Id = r.Role.Id,
                    Name = r.Role.Name,
                    RolePermissionRelations = r.Role.RolePermissionRelations.Select(rp => new RolePermissionRelation
                    {
                        Id = rp.Id,
                        PermissionId = rp.PermissionId,
                        RoleId = rp.RoleId,
                        Permission = new Permission
                        {
                            Id = rp.Permission.Id,
                            Name = rp.Permission.Name,
                            PermissionGroup = rp.Permission.PermissionGroup
                        }
                    }).ToList()
                },
                UserId = r.UserId,
                RoleId = r.RoleId

            }).ToList(),
        };

        public static Expression<Func<User, User>> ChangePassword { get; } = u => new User
        {
            Id = u.Id,
            UserName = u.UserName,
            Email = u.Email,
            PasswordHash = u.PasswordHash,
            Sessions = u.Sessions.Select(s => new UserSession
            {
                Id = s.Id,
                AccessToken = s.AccessToken,
                RefreshToken = s.RefreshToken,
                UserId = s.UserId,
                DeviceId = s.DeviceId,
                CreationDate = s.CreationDate,
                ExpiryDate = s.ExpiryDate,
                UserName = s.UserName,
                IpAddress = s.IpAddress
            }).ToList(),
            IsDeleted = u.IsDeleted,
        };

        public static Expression<Func<User, User>> UpdateUserWithSessions { get; } = u => new User
        {
            Id = u.Id,
            UserName = u.UserName,
            Sessions = u.Sessions.Select(s => new UserSession
            {
                Id = s.Id,
                AccessToken = s.AccessToken,
                RefreshToken = s.RefreshToken,
                UserId = s.UserId,
                DeviceId = s.DeviceId,
                CreationDate = s.CreationDate,
                ExpiryDate = s.ExpiryDate,
                UserName = s.UserName,
                IpAddress = s.IpAddress,
            }).ToList(),
            IsDeleted = u.IsDeleted,
        };

        public static Expression<Func<User, User>> CurrentUserCheck { get; } = u => new User
        {
            Id = u.Id,
            UserName = u.UserName,
        };

        public static Expression<Func<User, User>> CreateNotification { get; } = u => new User
        {
            Id = u.Id,
            AllowedNotifications = u.AllowedNotifications,
            UserName = u.UserName,
        };
    }

    #endregion
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}
