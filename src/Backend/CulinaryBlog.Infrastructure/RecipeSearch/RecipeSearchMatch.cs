using CulinaryBlog.Domain.Enums;

namespace CulinaryBlog.Infrastructure.RecipeSearch;

internal sealed class RecipeSearchMatch
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int PrepTimeMinutes { get; set; }
    public int CookTimeMinutes { get; set; }
    public int Servings { get; set; }
    public Difficulty Difficulty { get; set; }
    public RecipeStatus Status { get; set; }
    public DateTimeOffset? PublishedAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public Guid? CategoryId { get; set; }
    public double RelevanceScore { get; set; }
}
