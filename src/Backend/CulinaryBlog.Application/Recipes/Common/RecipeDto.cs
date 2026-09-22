using CulinaryBlog.Domain.Enums;

namespace CulinaryBlog.Application.Recipes.Common;

public class RecipeDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = default!;
    public string Slug { get; init; } = default!;
    public string Description { get; init; } = default!;
    public int PrepTimeMinutes { get; init; }
    public int CookTimeMinutes { get; init; }
    public int Servings { get; init; }
    public Difficulty Difficulty { get; init; }
    public RecipeStatus Status { get; init; }
    public DateTime? PublishedAt { get; init; }
    public string AuthorId { get; init; } = default!;
    public Guid? CategoryId { get; init; }
    public uint Version { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }

    public List<RecipeStepDto> Steps { get; init; } = [];
    public List<RecipeIngredientDto> Ingredients { get; init; } = [];
}

public class RecipeStepDto
{
    public Guid Id { get; init; }
    public int StepNumber { get; init; }
    public string? Title { get; init; }
    public string Description { get; init; } = default!;
    public int DurationMinutes { get; init; }
}

public class RecipeIngredientDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = default!;
    public decimal? Quantity { get; init; }
    public string? Unit { get; init; }
    public int OrderIndex { get; init; }
}