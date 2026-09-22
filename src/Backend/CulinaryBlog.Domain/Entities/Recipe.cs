using CulinaryBlog.Domain.Common;
using CulinaryBlog.Domain.Enums;

namespace CulinaryBlog.Domain.Entities;

public class Recipe : BaseEntity, ISoftDeletable
{
    public string Title { get; set; } = default!;
    public string Slug { get; set; } = default!;
    public string Description { get; set; } = default!;

    public int PrepTimeMinutes { get; set; }
    public int CookTimeMinutes { get; set; }
    public int Servings { get; set; }
    public Difficulty Difficulty { get; set; }

    public RecipeStatus Status { get; set; } = RecipeStatus.Draft;
    public DateTimeOffset? PublishedAt { get; set; }

    public string AuthorId { get; set; } = default!;
    public Guid? CategoryId { get; set; }

    public RecipeNutrition? Nutrition { get; set; }

    public ICollection<RecipeStep> Steps { get; set; } = new List<RecipeStep>();
    public ICollection<RecipeIngredient> Ingredients { get; set; } = new List<RecipeIngredient>();

    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
}