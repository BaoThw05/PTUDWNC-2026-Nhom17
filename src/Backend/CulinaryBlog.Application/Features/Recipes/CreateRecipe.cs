using CulinaryBlog.Application.Abstractions;
using CulinaryBlog.Domain.Entities;
using CulinaryBlog.Domain.Enums;
using FluentValidation;
using MediatR;

namespace CulinaryBlog.Application.Features.Recipes;

public sealed record CreateRecipeCommand : IRequest<Guid>
{
    public string Title { get; init; } = default!;
    public string Description { get; init; } = default!;
    public int PrepTimeMinutes { get; init; }
    public int CookTimeMinutes { get; init; }
    public int Servings { get; init; }
    public Difficulty Difficulty { get; init; }
    public string AuthorId { get; init; } = default!;
}

public sealed class CreateRecipeCommandValidator : AbstractValidator<CreateRecipeCommand>
{
    public CreateRecipeCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty().Length(5, 200);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(2000);
        RuleFor(x => x.PrepTimeMinutes).GreaterThan(0);
        RuleFor(x => x.CookTimeMinutes).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Servings).GreaterThan(0);
        RuleFor(x => x.Difficulty).IsInEnum();
        RuleFor(x => x.AuthorId).NotEmpty();
    }
}

public sealed class CreateRecipeCommandHandler(IAppDbContext db) : IRequestHandler<CreateRecipeCommand, Guid>
{
    public async Task<Guid> Handle(CreateRecipeCommand request, CancellationToken cancellationToken)
    {
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
            AuthorId = request.AuthorId,
            Status = RecipeStatus.Draft
        };

        db.Add(recipe);
        await db.SaveChangesAsync(cancellationToken);

        return recipe.Id;
    }
}
