using Microsoft.EntityFrameworkCore;
using Milvasoft.DataAccess.EfCore.Bulk.DbContextBase;
using Milvasoft.DataAccess.EfCore.Configuration;
using Milvonion.Domain;
using Milvonion.Domain.ContentManagement;
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
    public DbSet<MigrationHistory> MigrationHistory { get; set; }
    public DbSet<EfMigrationHistory> EfMigrationHistory { get; set; }
    public DbSet<ActivityLog> ActivityLogs { get; set; }
    public DbSet<Permission> Permissions { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<RolePermissionRelation> RolePermissionRelations { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<UserRoleRelation> UserRoleRelations { get; set; }
    public DbSet<UserSession> UserSessions { get; set; }
    public DbSet<UserSessionHistory> UserSessionHistories { get; set; }
    public DbSet<MenuItem> MenuItems { get; set; }
    public DbSet<MenuGroup> MenuGroups { get; set; }
    public DbSet<Page> Pages { get; set; }
    public DbSet<PageAction> PageActions { get; set; }
    public DbSet<Content> Contents { get; set; }
    public DbSet<Media> Medias { get; set; }
    public DbSet<ResourceGroup> ResourceGroups { get; set; }
    public DbSet<Namespace> Namespaces { get; set; }
    public DbSet<Language> Languages { get; set; }
    public DbSet<InternalNotification> InternalNotifications { get; set; }
    public DbSet<IdempotencyKey> IdempotencyKeys { get; set; }
    public DbSet<FeatureFlag> FeatureFlags { get; set; }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

    /// <summary>
    /// Configures what attributes cannot express, in addition to the base Milvasoft model configuration.
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // The e-mail is optional (empty for the seeded root user), so only a set e-mail must be unique; NULLs are
        // distinct anyway. The index name is what UserUniqueConstraintHelper maps to MessageKey.EmailExists.
        modelBuilder.Entity<User>()
                    .HasIndex(u => u.NormalizedEmail)
                    .IsUnique()
                    .HasFilter("\"NormalizedEmail\" <> ''");

        // Idempotency keys are scoped to their caller: the composite key is the dedupe itself, so a retry
        // conflicts on insert and one user can never consume another's key. No surrogate id, no navigations.
        modelBuilder.Entity<IdempotencyKey>()
                    .HasKey(k => new { k.UserId, k.Key });
    }
}