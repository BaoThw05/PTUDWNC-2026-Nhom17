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
        string prefixQuery,
        Guid? categoryId,
        Difficulty? difficulty,
        int? maxCookTime,
        int? minServings,
        string? sort,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var difficultyValue = difficulty.HasValue ? (int?)difficulty.Value : null;
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
                    recipe."CreatedAt",
                    recipe."CategoryId",
                    (
                        CASE
                            WHEN recipe."SearchVector" @@ to_tsquery('public.vietnamese', {prefixQuery})
                                THEN ts_rank(
                                    recipe."SearchVector",
                                    to_tsquery('public.vietnamese', {prefixQuery}))::double precision
                            ELSE 0::double precision
                        END + similarity(recipe."SearchText", {normalizedTerm})::double precision
                    ) AS "RelevanceScore"
                FROM "Recipes" AS recipe
                WHERE recipe."Status" = {(int)RecipeStatus.Published}
                  AND recipe."IsDeleted" = false
                  AND (
                      recipe."SearchVector" @@ to_tsquery('public.vietnamese', {prefixQuery})
                      OR recipe."SearchText" % {normalizedTerm}
                  )
                  AND (CAST({categoryId} AS uuid) IS NULL OR recipe."CategoryId" = CAST({categoryId} AS uuid))
                  AND (CAST({difficultyValue} AS integer) IS NULL OR recipe."Difficulty" = CAST({difficultyValue} AS integer))
                  AND (CAST({maxCookTime} AS integer) IS NULL OR recipe."CookTimeMinutes" <= CAST({maxCookTime} AS integer))
                  AND (CAST({minServings} AS integer) IS NULL OR recipe."Servings" >= CAST({minServings} AS integer))
                """)
            .AsNoTracking();

        var totalCount = await matches.CountAsync(cancellationToken);
        var sorted = ApplySort(matches, sort);
        var items = await sorted
            .ThenByDescending(match => match.RelevanceScore)
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

    private static IOrderedQueryable<RecipeSearchMatch> ApplySort(
        IQueryable<RecipeSearchMatch> matches,
        string? sort)
    {
        if (string.IsNullOrWhiteSpace(sort))
        {
            return matches.OrderByDescending(match => match.RelevanceScore);
        }

        var descending = sort.StartsWith("-", StringComparison.Ordinal);
        var field = descending ? sort[1..] : sort;

        return (field.ToLowerInvariant(), descending) switch
        {
            ("createdat", true) => matches.OrderByDescending(match => match.CreatedAt),
            ("createdat", false) => matches.OrderBy(match => match.CreatedAt),
            ("title", true) => matches.OrderByDescending(match => match.Title),
            ("title", false) => matches.OrderBy(match => match.Title),
            ("cooktime", true) => matches.OrderByDescending(match => match.CookTimeMinutes),
            ("cooktime", false) => matches.OrderBy(match => match.CookTimeMinutes),
            _ => matches.OrderByDescending(match => match.RelevanceScore)
        };
    }
}
