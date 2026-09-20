using CulinaryBlog.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CulinaryBlog.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Recipe> Recipes { get; }
    DbSet<RecipeStep> RecipeSteps { get; }
    DbSet<RecipeIngredient> RecipeIngredients { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}