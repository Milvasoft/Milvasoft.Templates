using Microsoft.EntityFrameworkCore;

namespace Milvonion.Infrastructure.Persistence.Context;

/// <summary>
/// Extension methods for MilvonionDbContexts
/// </summary>
public static class ModelBuilderExtensions
{
    /// <summary>
    /// Use UTC for datetime types.
    /// </summary>
    /// <param name="modelBuilder"></param>
    public static ModelBuilder UseUtcDateTime(this ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                if (property.ClrType == typeof(DateTime))
                {
                    modelBuilder.Entity(entityType.ClrType)
                                .Property<DateTime>(property.Name)
                                .HasConversion(v => v.ToUniversalTime(), v => DateTime.SpecifyKind(v, DateTimeKind.Utc));
                }
                else if (property.ClrType == typeof(DateTime?))
                {
                    modelBuilder.Entity(entityType.ClrType)
                                .Property<DateTime?>(property.Name)
                                .HasConversion(v => v.HasValue ? v.Value.ToUniversalTime() : v, v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v);
                }
                else if (property.ClrType == typeof(DateTimeOffset))
                {
                    modelBuilder.Entity(entityType.ClrType)
                                .Property<DateTimeOffset>(property.Name)
                                .HasConversion(v => v.ToUniversalTime(), v => v);
                }
                else if (property.ClrType == typeof(DateTimeOffset?))
                {
                    modelBuilder.Entity(entityType.ClrType)
                                .Property<DateTimeOffset?>(property.Name)
                                .HasConversion(v => v.HasValue ? v.Value.ToUniversalTime() : v, v => v);
                }
            }
        }

        return modelBuilder;
    }
}
