using CulinaryBlog.Application.Common.Models;
using CulinaryBlog.Domain.Enums;

namespace CulinaryBlog.Application.Features.RecipeSearch;

public interface IRecipeSearchRepository
{
    Task<PagedResult<RecipeSummaryDto>> SearchAsync(
        string normalizedTerm,
        Guid? categoryId,
        Difficulty? difficulty,
        int? maxCookTime,
        int? minServings,
        int page,
        int pageSize,
        CancellationToken cancellationToken);
}
