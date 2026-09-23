using CulinaryBlog.Application.Abstractions;
using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Domain.Entities;
using CulinaryBlog.Domain.Enums;
using FluentValidation;
using MediatR;
using ValidationException = CulinaryBlog.Application.Common.Exceptions.ValidationException;

namespace CulinaryBlog.Application.Features.Recipes;

public sealed record UpdateRecipeCommand : IRequest
{
    public Guid Id { get; init; }
    public string Title { get; init; } = default!;
    public string Description { get; init; } = default!;
    public int PrepTimeMinutes { get; init; }
    public int CookTimeMinutes { get; init; }
    public int Servings { get; init; }
    public Difficulty Difficulty { get; init; }
    public Guid? CategoryId { get; init; }
    public RecipeNutritionDto? Nutrition { get; init; }
    public uint Version { get; init; }
}

public sealed class UpdateRecipeCommandValidator : AbstractValidator<UpdateRecipeCommand>
{
    public UpdateRecipeCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Title).NotEmpty().Length(5, 200);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(2000);
        RuleFor(x => x.PrepTimeMinutes).GreaterThan(0);
        RuleFor(x => x.CookTimeMinutes).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Servings).GreaterThan(0);
        RuleFor(x => x.Difficulty).IsInEnum();
        RuleFor(x => x.Version).GreaterThan(0u).WithMessage("Version phải lớn hơn 0.");

        When(x => x.Nutrition != null, () =>
        {
            RuleFor(x => x.Nutrition!.Calories).GreaterThanOrEqualTo(0).When(n => n.Nutrition!.Calories.HasValue);
            RuleFor(x => x.Nutrition!.ProteinGrams).GreaterThanOrEqualTo(0).When(n => n.Nutrition!.ProteinGrams.HasValue);
            RuleFor(x => x.Nutrition!.FatGrams).GreaterThanOrEqualTo(0).When(n => n.Nutrition!.FatGrams.HasValue);
            RuleFor(x => x.Nutrition!.CarbsGrams).GreaterThanOrEqualTo(0).When(n => n.Nutrition!.CarbsGrams.HasValue);
            RuleFor(x => x.Nutrition!.FiberGrams).GreaterThanOrEqualTo(0).When(n => n.Nutrition!.FiberGrams.HasValue);
            RuleFor(x => x.Nutrition!.SugarGrams).GreaterThanOrEqualTo(0).When(n => n.Nutrition!.SugarGrams.HasValue);
        });
    }
}

public sealed class UpdateRecipeCommandHandler(
    IAppDbContext db,
    IRecipeAuthorizationHandler authorizationHandler,
    ICategoryValidator categoryValidator) : IRequestHandler<UpdateRecipeCommand>
{
    public async Task Handle(UpdateRecipeCommand request, CancellationToken cancellationToken)
    {
        var recipe = db.Recipes.FirstOrDefault(r => r.Id == request.Id)
            ?? throw new NotFoundException(
                $"Không tìm thấy công thức có id '{request.Id}'", RecipeErrorCodes.RecipeNotFound);

        // 2.10 & NFR-SEC-006: Kiểm tra quyền sở hữu bài viết (chỉ tác giả hoặc Admin)
        authorizationHandler.EnsureCanModify(recipe);

        // 2.10: Kiểm tra danh mục hợp lệ nếu có truyền
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

        // 2.10: Sinh lại slug khi đổi tiêu đề và bài CHƯA TỪNG publish (PublishedAt == null)
        if (recipe.Title != request.Title && recipe.PublishedAt == null)
        {
            var baseSlug = SlugHelper.GenerateSlug(request.Title);
            var slug = baseSlug;
            var counter = 2;

            while (db.Recipes.Any(r => r.Id != recipe.Id && r.Slug == slug))
            {
                slug = $"{baseSlug}-{counter}";
                counter++;
            }

            recipe.Slug = slug;
        }

        recipe.Title = request.Title;
        recipe.Description = request.Description;
        recipe.PrepTimeMinutes = request.PrepTimeMinutes;
        recipe.CookTimeMinutes = request.CookTimeMinutes;
        recipe.Servings = request.Servings;
        recipe.Difficulty = request.Difficulty;
        recipe.CategoryId = request.CategoryId;
        recipe.UpdatedAt = DateTimeOffset.UtcNow;

        if (request.Nutrition is not null)
        {
            recipe.Nutrition ??= new RecipeNutrition();
            recipe.Nutrition.Calories = request.Nutrition.Calories;
            recipe.Nutrition.ProteinGrams = request.Nutrition.ProteinGrams;
            recipe.Nutrition.FatGrams = request.Nutrition.FatGrams;
            recipe.Nutrition.CarbsGrams = request.Nutrition.CarbsGrams;
            recipe.Nutrition.FiberGrams = request.Nutrition.FiberGrams;
            recipe.Nutrition.SugarGrams = request.Nutrition.SugarGrams;
        }
        else
        {
            recipe.Nutrition = null;
        }

        // S-04: gán version client gửi lên làm "giá trị gốc" để EF Core so sánh với xmin thật trong DB
        db.SetOriginalVersion(recipe, request.Version);

        await db.SaveChangesAsync(cancellationToken);
    }
}
