using CulinaryBlog.Application.Features.Recipes;
using CulinaryBlog.Domain.Enums;

namespace CulinaryBlog.UnitTests.Recipes;

public class RecipeValidationTests
{
    private readonly CreateRecipeCommandValidator _createValidator = new();
    private readonly UpdateRecipeCommandValidator _updateValidator = new();

    [Fact]
    public void CreateRecipe_WithValidCommand_ShouldPassValidation()
    {
        var command = new CreateRecipeCommand
        {
            Title = "Bún bò Huế chuẩn vị",
            Description = "Món bún bò cay nồng thơm mùi sả mắm ruốc đặc trưng xứ Huế.",
            PrepTimeMinutes = 30,
            CookTimeMinutes = 120,
            Servings = 4,
            Difficulty = Difficulty.Medium,
            Nutrition = new RecipeNutritionDto
            {
                Calories = 550,
                ProteinGrams = 32.5m,
                FatGrams = 18.2m,
                CarbsGrams = 65.0m
            },
            Steps =
            [
                new CreateRecipeStepDto { Title = "Sơ chế", Description = "Rửa sạch thịt bò và luộc sơ qua nước sôi.", DurationMinutes = 15 }
            ],
            Ingredients =
            [
                new CreateRecipeIngredientDto { Name = "Bắp bò", Quantity = 500, Unit = "g" }
            ]
        };

        var result = _createValidator.Validate(command);

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("Pho")] // < 5 chars
    public void CreateRecipe_WithInvalidTitle_ShouldFailValidation(string invalidTitle)
    {
        var command = new CreateRecipeCommand
        {
            Title = invalidTitle,
            Description = "Mô tả hợp lệ cho món ăn.",
            PrepTimeMinutes = 10,
            CookTimeMinutes = 20,
            Servings = 2,
            Difficulty = Difficulty.Easy
        };

        var result = _createValidator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateRecipeCommand.Title));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void CreateRecipe_WithInvalidPrepTimeOrServings_ShouldFailValidation(int invalidValue)
    {
        var command = new CreateRecipeCommand
        {
            Title = "Món ăn thử nghiệm",
            Description = "Mô tả hợp lệ cho món ăn.",
            PrepTimeMinutes = invalidValue,
            CookTimeMinutes = 20,
            Servings = invalidValue,
            Difficulty = Difficulty.Easy
        };

        var result = _createValidator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateRecipeCommand.PrepTimeMinutes));
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateRecipeCommand.Servings));
    }

    [Fact]
    public void CreateRecipe_WithNegativeNutrition_ShouldFailValidation()
    {
        var command = new CreateRecipeCommand
        {
            Title = "Món ăn dinh dưỡng",
            Description = "Mô tả dinh dưỡng",
            PrepTimeMinutes = 15,
            CookTimeMinutes = 30,
            Servings = 2,
            Difficulty = Difficulty.Easy,
            Nutrition = new RecipeNutritionDto
            {
                Calories = -10,
                ProteinGrams = -5m
            }
        };

        var result = _createValidator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName.StartsWith("Nutrition.Calories"));
        Assert.Contains(result.Errors, e => e.PropertyName.StartsWith("Nutrition.ProteinGrams"));
    }

    [Fact]
    public void CreateRecipe_WithEmptyStepDescription_ShouldFailValidation()
    {
        var command = new CreateRecipeCommand
        {
            Title = "Món ăn có bước",
            Description = "Mô tả",
            PrepTimeMinutes = 10,
            CookTimeMinutes = 20,
            Servings = 2,
            Difficulty = Difficulty.Easy,
            Steps =
            [
                new CreateRecipeStepDto { Title = "Bước 1", Description = "" }
            ]
        };

        var result = _createValidator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName.Contains(nameof(CreateRecipeStepDto.Description)));
    }

    [Fact]
    public void UpdateRecipe_WithZeroVersion_ShouldFailValidation()
    {
        var command = new UpdateRecipeCommand
        {
            Id = Guid.NewGuid(),
            Title = "Cập nhật công thức nấu ăn",
            Description = "Mô tả chi tiết đã cập nhật",
            PrepTimeMinutes = 20,
            CookTimeMinutes = 40,
            Servings = 4,
            Difficulty = Difficulty.Medium,
            Version = 0 // Bắt buộc > 0 (2.10)
        };

        var result = _updateValidator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateRecipeCommand.Version));
    }

    [Fact]
    public void UpdateRecipe_WithValidVersion_ShouldPassValidation()
    {
        var command = new UpdateRecipeCommand
        {
            Id = Guid.NewGuid(),
            Title = "Cập nhật công thức nấu ăn",
            Description = "Mô tả chi tiết đã cập nhật",
            PrepTimeMinutes = 20,
            CookTimeMinutes = 40,
            Servings = 4,
            Difficulty = Difficulty.Medium,
            Version = 105
        };

        var result = _updateValidator.Validate(command);

        Assert.True(result.IsValid);
    }
}
