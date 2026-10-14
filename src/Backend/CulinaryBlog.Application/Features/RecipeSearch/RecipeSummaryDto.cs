using CulinaryBlog.Domain.Enums;

namespace CulinaryBlog.Application.Features.RecipeSearch;

public sealed record RecipeSummaryDto(
    Guid Id,
    string Title,
    string Slug,
    string Description,
    int PrepTimeMinutes,
    int CookTimeMinutes,
    int Servings,
    Difficulty Difficulty,
    RecipeStatus Status,
    DateTimeOffset? PublishedAt,
    Guid? CategoryId,
    string? ThumbnailUrl,
    double? RelevanceScore = null);
