using Microsoft.EntityFrameworkCore;
using Milvasoft.DataAccess.EfCore.Bulk.DbContextBase;
using Milvasoft.DataAccess.EfCore.Configuration;
using Milvonion.Domain;
using Milvonion.Domain.UI;

namespace Milvonion.Infrastructure.Persistence.Context;

/// <summary>
/// Handles all database operations. Inherits <see cref="MilvonionDbContext"/>
/// </summary>
/// 
/// <remarks>
/// <para> You must register <see cref="IDataAccessConfiguration"/> in your application startup. </para>
/// <para> If <see cref="IDataAccessConfiguration"/>'s AuditDeleter, AuditModifier or AuditCreator is true
///        and HttpMethod is POST,PUT or DELETE it will gets performer user in constructor from database.
///        This can affect performance little bit. But you want audit every record easily you must use this :( </para>
/// </remarks>
public class MilvonionDbContext(DbContextOptions options) : MilvaBulkDbContext(options)
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public DbSet<ActivityLog> ActivityLogs { get; set; }
    public DbSet<ApiLog> ApiLogs { get; set; }
    public DbSet<MethodLog> MethodLogs { get; set; }
    public DbSet<Permission> Permissions { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<RolePermissionRelation> RolePermissionRelations { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<UserRoleRelation> UserRoleRelations { get; set; }
    public DbSet<UserSession> UserSessions { get; set; }
    public DbSet<MenuItem> MenuItems { get; set; }
    public DbSet<MenuGroup> MenuGroups { get; set; }
    public DbSet<Page> Pages { get; set; }
    public DbSet<PageAction> PageActions { get; set; }
    public DbSet<MigrationHistory> MigrationHistory { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

    /// <summary>
    /// Overrided the OnModelCreating for custom configurations to database.
    /// </summary>
    /// <param name="modelBuilder"></param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        if (_useUtcForDateTimes)
            modelBuilder.UseUtcDateTime();

        base.OnModelCreating(modelBuilder);
    }
}