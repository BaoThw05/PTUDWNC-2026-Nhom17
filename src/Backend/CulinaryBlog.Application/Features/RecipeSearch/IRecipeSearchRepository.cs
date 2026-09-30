using CulinaryBlog.Application.Common.Models;
using CulinaryBlog.Domain.Enums;

namespace CulinaryBlog.Application.Features.RecipeSearch;

public interface IRecipeSearchRepository
{
    Task<PagedResult<RecipeSummaryDto>> SearchAsync(
        string normalizedTerm,
        string prefixQuery,
        Guid? categoryId,
        Difficulty? difficulty,
        int? maxCookTime,
        int? minServings,
        string? sort,
        int page,
        int pageSize,
        CancellationToken cancellationToken);
}
