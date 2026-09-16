using CulinaryBlog.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace CulinaryBlog.Infrastructure.Persistence;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Module đặt IEntityTypeConfiguration<T> trong thư mục Infrastructure/<Module>/ nên không cần sửa file này.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        MapVersionToXmin(modelBuilder);
    }

    // S-04: dùng cột hệ thống xmin làm concurrency token nên không cần cột RowVersion riêng.
    private static void MapVersionToXmin(ModelBuilder modelBuilder)
    {
        var entityTypes = modelBuilder.Model.GetEntityTypes()
            .Where(type => !type.IsOwned() && typeof(BaseEntity).IsAssignableFrom(type.ClrType));

        foreach (var entityType in entityTypes)
        {
            modelBuilder.Entity(entityType.ClrType)
                .Property(nameof(BaseEntity.Version))
                .IsRowVersion();
        }
    }
}
