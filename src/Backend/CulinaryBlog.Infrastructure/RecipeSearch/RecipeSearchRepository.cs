using CulinaryBlog.Application.Common.Models;
using CulinaryBlog.Application.Features.RecipeSearch;
using CulinaryBlog.Domain.Enums;
using CulinaryBlog.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CulinaryBlog.Infrastructure.RecipeSearch;

internal sealed class RecipeSearchRepository(AppDbContext db) : IRecipeSearchRepository
{
    public async Task<PagedResult<RecipeSummaryDto>> SearchAsync(
        string normalizedTerm,
        Guid? categoryId,
        Difficulty? difficulty,
        int? maxCookTime,
        int? minServings,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var difficultyValue = difficulty.HasValue ? (int?)difficulty.Value : null;
        var likePattern = $"%{normalizedTerm}%";
        var matches = db.Set<RecipeSearchMatch>()
            .FromSqlInterpolated($"""
                SELECT
                    recipe."Id",
                    recipe."Title",
                    recipe."Slug",
                    recipe."Description",
                    recipe."PrepTimeMinutes",
                    recipe."CookTimeMinutes",
                    recipe."Servings",
                    recipe."Difficulty",
                    recipe."Status",
                    recipe."PublishedAt",
                    recipe."CategoryId",
                    similarity(recipe."SearchText", {normalizedTerm})::double precision AS "RelevanceScore"
                FROM "Recipes" AS recipe
                WHERE recipe."Status" = {(int)RecipeStatus.Published}
                  AND recipe."IsDeleted" = false
                  AND (
                      recipe."SearchText" ILIKE {likePattern}
                      OR (recipe."SearchText" % {normalizedTerm}
                          AND similarity(recipe."SearchText", {normalizedTerm}) > 0.3)
                  )
                  AND (CAST({categoryId} AS uuid) IS NULL OR recipe."CategoryId" = CAST({categoryId} AS uuid))
                  AND (CAST({difficultyValue} AS integer) IS NULL OR recipe."Difficulty" = CAST({difficultyValue} AS integer))
                  AND (CAST({maxCookTime} AS integer) IS NULL OR recipe."CookTimeMinutes" <= CAST({maxCookTime} AS integer))
                  AND (CAST({minServings} AS integer) IS NULL OR recipe."Servings" >= CAST({minServings} AS integer))
                """)
            .AsNoTracking();

        var totalCount = await matches.CountAsync(cancellationToken);
        var items = await matches
            .OrderByDescending(match => match.RelevanceScore)
            .ThenBy(match => match.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(match => new RecipeSummaryDto(
                match.Id,
                match.Title,
                match.Slug,
                match.Description,
                match.PrepTimeMinutes,
                match.CookTimeMinutes,
                match.Servings,
                match.Difficulty,
                match.Status,
                match.PublishedAt,
                match.CategoryId,
                null,
                match.RelevanceScore))
            .ToListAsync(cancellationToken);

        return new PagedResult<RecipeSummaryDto>(items, page, pageSize, totalCount);
    }
}
