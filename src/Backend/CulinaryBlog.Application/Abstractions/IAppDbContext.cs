using CulinaryBlog.Domain.Common;
using CulinaryBlog.Domain.Entities;

namespace CulinaryBlog.Application.Abstractions;

public interface IAppDbContext
{
    IQueryable<Category> Categories { get; }
    IQueryable<Recipe> Recipes { get; }
    IQueryable<RecipeStep> RecipeSteps { get; }
    IQueryable<RecipeIngredient> RecipeIngredients { get; }

    // Dùng cho thùng rác (S-03): bỏ qua Global Query Filter để tìm được recipe đã xóa mềm.
    IQueryable<Recipe> RecipesIncludingDeleted { get; }

    void Add<TEntity>(TEntity entity) where TEntity : class;
    void Remove<TEntity>(TEntity entity) where TEntity : class;

    void SetOriginalVersion<TEntity>(TEntity entity, uint version) where TEntity : BaseEntity;

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
