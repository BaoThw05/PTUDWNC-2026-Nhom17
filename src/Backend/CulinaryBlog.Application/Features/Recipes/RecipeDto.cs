using CulinaryBlog.Domain.Enums;

namespace CulinaryBlog.Application.Features.Recipes;

public sealed class RecipeDto
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
    public DateTimeOffset? PublishedAt { get; init; }
    public string AuthorId { get; init; } = default!;
    public Guid? CategoryId { get; init; }
    public uint Version { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? UpdatedAt { get; init; }

    public RecipeNutritionDto? Nutrition { get; init; }
    public List<RecipeStepDto> Steps { get; init; } = [];
    public List<RecipeIngredientDto> Ingredients { get; init; } = [];
}

public sealed class RecipeNutritionDto
{
    public int? Calories { get; init; }
    public decimal? ProteinGrams { get; init; }
    public decimal? FatGrams { get; init; }
    public decimal? CarbsGrams { get; init; }
    public decimal? FiberGrams { get; init; }
    public decimal? SugarGrams { get; init; }
}

public sealed class RecipeStepDto
{
    public Guid Id { get; init; }
    public int StepNumber { get; init; }
    public string? Title { get; init; }
    public string Description { get; init; } = default!;
    public int DurationMinutes { get; init; }
}

public sealed class RecipeIngredientDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = default!;
    public decimal? Quantity { get; init; }
    public string? Unit { get; init; }
    public int OrderIndex { get; init; }
}

public sealed class CreateRecipeStepDto
{
    public string? Title { get; init; }
    public string Description { get; init; } = default!;
    public int DurationMinutes { get; init; }
}

public sealed class CreateRecipeIngredientDto
{
    public string Name { get; init; } = default!;
    public decimal? Quantity { get; init; }
    public string? Unit { get; init; }
}
