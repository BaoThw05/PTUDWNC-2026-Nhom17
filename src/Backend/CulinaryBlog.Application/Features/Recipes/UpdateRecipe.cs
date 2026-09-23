using CulinaryBlog.Application.Abstractions;
using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Domain.Enums;
using FluentValidation;
using MediatR;

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
    }
}

public sealed class UpdateRecipeCommandHandler(IAppDbContext db) : IRequestHandler<UpdateRecipeCommand>
{
    public async Task Handle(UpdateRecipeCommand request, CancellationToken cancellationToken)
    {
        var recipe = db.Recipes.FirstOrDefault(r => r.Id == request.Id)
            ?? throw new NotFoundException(
                $"Không tìm thấy công thức có id '{request.Id}'", RecipeErrorCodes.RecipeNotFound);

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

        // S-04: gán version client gui len lam "gia tri goc" de EF Core so sanh voi xmin that trong DB
        db.SetOriginalVersion(recipe, request.Version);

        await db.SaveChangesAsync(cancellationToken);
    }
}
