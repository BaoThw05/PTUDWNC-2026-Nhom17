using CulinaryBlog.Application.Abstractions;
using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Domain.Entities;
using FluentValidation;
using MediatR;

namespace CulinaryBlog.Application.Features.Recipes;

public sealed record AddRecipeIngredientCommand : IRequest<Guid>
{
    public Guid RecipeId { get; init; }
    public string Name { get; init; } = default!;
    public decimal? Quantity { get; init; }
    public string? Unit { get; init; }
}

public sealed class AddRecipeIngredientCommandValidator : AbstractValidator<AddRecipeIngredientCommand>
{
    public AddRecipeIngredientCommandValidator()
    {
        RuleFor(x => x.RecipeId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Quantity).GreaterThan(0).When(x => x.Quantity.HasValue);
    }
}

public sealed class AddRecipeIngredientCommandHandler(IAppDbContext db)
    : IRequestHandler<AddRecipeIngredientCommand, Guid>
{
    public async Task<Guid> Handle(AddRecipeIngredientCommand request, CancellationToken cancellationToken)
    {
        var recipe = db.Recipes.FirstOrDefault(r => r.Id == request.RecipeId)
            ?? throw new NotFoundException(
                $"Không tìm thấy công thức có id '{request.RecipeId}'", RecipeErrorCodes.RecipeNotFound);

        var nextOrderIndex = db.RecipeIngredients.Count(i => i.RecipeId == request.RecipeId);

        var ingredient = new RecipeIngredient
        {
            RecipeId = request.RecipeId,
            Name = request.Name,
            Quantity = request.Quantity,
            Unit = request.Unit,
            OrderIndex = nextOrderIndex
        };

        db.Add(ingredient);

        // S-04 / C-28: Cập nhật Recipe.UpdatedAt để xmin của Recipe cha thay đổi theo
        recipe.UpdatedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(cancellationToken);

        return ingredient.Id;
    }
}
