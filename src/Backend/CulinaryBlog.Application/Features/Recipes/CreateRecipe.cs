using CulinaryBlog.Application.Abstractions;
using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Domain.Entities;
using CulinaryBlog.Domain.Enums;
using FluentValidation;
using MediatR;
using ValidationException = CulinaryBlog.Application.Common.Exceptions.ValidationException;

namespace CulinaryBlog.Application.Features.Recipes;

public sealed record CreateRecipeCommand : IRequest<Guid>
{
    public string Title { get; init; } = default!;
    public string Description { get; init; } = default!;
    public int PrepTimeMinutes { get; init; }
    public int CookTimeMinutes { get; init; }
    public int Servings { get; init; }
    public Difficulty Difficulty { get; init; }
    public string? AuthorId { get; init; }
    public Guid? CategoryId { get; init; }
    public RecipeNutritionDto? Nutrition { get; init; }
    public List<CreateRecipeStepDto>? Steps { get; init; }
    public List<CreateRecipeIngredientDto>? Ingredients { get; init; }
}

public sealed class CreateRecipeCommandValidator : AbstractValidator<CreateRecipeCommand>
{
    public CreateRecipeCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty().Length(5, 200);
        RuleFor(x => x.Description).NotEmpty().Length(1, 2000);
        RuleFor(x => x.PrepTimeMinutes).GreaterThan(0);
        RuleFor(x => x.CookTimeMinutes).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Servings).GreaterThan(0);
        RuleFor(x => x.Difficulty).IsInEnum();

        When(x => x.Nutrition != null, () =>
        {
            RuleFor(x => x.Nutrition!.Calories).GreaterThanOrEqualTo(0).When(n => n.Nutrition!.Calories.HasValue);
            RuleFor(x => x.Nutrition!.ProteinGrams).GreaterThanOrEqualTo(0).When(n => n.Nutrition!.ProteinGrams.HasValue);
            RuleFor(x => x.Nutrition!.FatGrams).GreaterThanOrEqualTo(0).When(n => n.Nutrition!.FatGrams.HasValue);
            RuleFor(x => x.Nutrition!.CarbsGrams).GreaterThanOrEqualTo(0).When(n => n.Nutrition!.CarbsGrams.HasValue);
            RuleFor(x => x.Nutrition!.FiberGrams).GreaterThanOrEqualTo(0).When(n => n.Nutrition!.FiberGrams.HasValue);
            RuleFor(x => x.Nutrition!.SugarGrams).GreaterThanOrEqualTo(0).When(n => n.Nutrition!.SugarGrams.HasValue);
        });

        RuleForEach(x => x.Steps).ChildRules(step =>
        {
            step.RuleFor(s => s.Description).NotEmpty().Length(1, 2000);
            step.RuleFor(s => s.Title).MaximumLength(200);
            step.RuleFor(s => s.DurationMinutes).GreaterThanOrEqualTo(0);
        });

        RuleForEach(x => x.Ingredients).ChildRules(ing =>
        {
            ing.RuleFor(i => i.Name).NotEmpty().Length(1, 200);
            ing.RuleFor(i => i.Quantity).GreaterThan(0).When(i => i.Quantity.HasValue);
            ing.RuleFor(i => i.Unit).MaximumLength(50);
        });
    }
}

public sealed class CreateRecipeCommandHandler(
    IAppDbContext db,
    ICurrentUser currentUser,
    ICategoryValidator categoryValidator) : IRequestHandler<CreateRecipeCommand, Guid>
{
    public async Task<Guid> Handle(CreateRecipeCommand request, CancellationToken cancellationToken)
    {
        var authorId = !string.IsNullOrWhiteSpace(request.AuthorId)
            ? request.AuthorId
            : currentUser.UserId?.ToString();

        if (string.IsNullOrWhiteSpace(authorId))
        {
            throw new UnauthorizedException("Bạn cần đăng nhập để tạo công thức.");
        }

        if (request.CategoryId.HasValue)
        {
            var categoryExists = await categoryValidator.ExistsAsync(request.CategoryId.Value, cancellationToken);
            if (!categoryExists)
            {
                throw new ValidationException(
                    RecipeErrorCodes.CategoryInvalid,
                    $"Danh mục có id '{request.CategoryId.Value}' không tồn tại hoặc không hợp lệ.");
            }
        }

        var baseSlug = SlugHelper.GenerateSlug(request.Title);
        var slug = baseSlug;
        var counter = 2;

        while (db.Recipes.Any(r => r.Slug == slug))
        {
            slug = $"{baseSlug}-{counter}";
            counter++;
        }

        var recipe = new Recipe
        {
            Title = request.Title,
            Slug = slug,
            Description = request.Description,
            PrepTimeMinutes = request.PrepTimeMinutes,
            CookTimeMinutes = request.CookTimeMinutes,
            Servings = request.Servings,
            Difficulty = request.Difficulty,
            AuthorId = authorId,
            CategoryId = request.CategoryId,
            Status = RecipeStatus.Draft
        };

        if (request.Nutrition is not null)
        {
            recipe.Nutrition = new RecipeNutrition
            {
                Calories = request.Nutrition.Calories,
                ProteinGrams = request.Nutrition.ProteinGrams,
                FatGrams = request.Nutrition.FatGrams,
                CarbsGrams = request.Nutrition.CarbsGrams,
                FiberGrams = request.Nutrition.FiberGrams,
                SugarGrams = request.Nutrition.SugarGrams
            };
        }

        if (request.Steps is { Count: > 0 })
        {
            var stepNumber = 1;
            foreach (var s in request.Steps)
            {
                recipe.Steps.Add(new RecipeStep
                {
                    StepNumber = stepNumber++,
                    Title = s.Title,
                    Description = s.Description,
                    DurationMinutes = s.DurationMinutes
                });
            }
        }

        if (request.Ingredients is { Count: > 0 })
        {
            var orderIndex = 0;
            foreach (var i in request.Ingredients)
            {
                recipe.Ingredients.Add(new RecipeIngredient
                {
                    Name = i.Name,
                    Quantity = i.Quantity,
                    Unit = i.Unit,
                    OrderIndex = orderIndex++
                });
            }
        }

        db.Add(recipe);
        await db.SaveChangesAsync(cancellationToken);

        return recipe.Id;
    }
}
