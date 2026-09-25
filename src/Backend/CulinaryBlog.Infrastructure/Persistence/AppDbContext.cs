using CulinaryBlog.Application.Abstractions;
using CulinaryBlog.Domain.Auth;
using CulinaryBlog.Domain.Common;
using CulinaryBlog.Domain.Entities;
using CulinaryBlog.Infrastructure.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CulinaryBlog.Infrastructure.Persistence;

// Kế thừa IdentityDbContext để bảng người dùng nằm chung DbContext, các module khác tạo khóa ngoại tới Users được.
public sealed class AppDbContext(DbContextOptions<AppDbContext> options)
    : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>(options), IAppDbContext
{
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    public DbSet<Recipe> Recipes => Set<Recipe>();
    public DbSet<RecipeStep> RecipeSteps => Set<RecipeStep>();
    public DbSet<RecipeIngredient> RecipeIngredients => Set<RecipeIngredient>();

    IQueryable<Recipe> IAppDbContext.Recipes => Recipes;
    IQueryable<RecipeStep> IAppDbContext.RecipeSteps => RecipeSteps;
    IQueryable<RecipeIngredient> IAppDbContext.RecipeIngredients => RecipeIngredients;
    IQueryable<Recipe> IAppDbContext.RecipesIncludingDeleted => Recipes.IgnoreQueryFilters();

    public new void Add<TEntity>(TEntity entity) where TEntity : class => Set<TEntity>().Add(entity);

    public new void Remove<TEntity>(TEntity entity) where TEntity : class => Set<TEntity>().Remove(entity);

    public void SetOriginalVersion<TEntity>(TEntity entity, uint version) where TEntity : BaseEntity =>
        Entry(entity).Property(e => e.Version).OriginalValue = version;

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
