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

public sealed class AddRecipeIngredientCommandHandler(
    IAppDbContext db,
    IRecipeAuthorizationHandler authorizationHandler) : IRequestHandler<AddRecipeIngredientCommand, Guid>
{
    public async Task<Guid> Handle(AddRecipeIngredientCommand request, CancellationToken cancellationToken)
    {
        var recipe = db.Recipes.FirstOrDefault(r => r.Id == request.RecipeId)
            ?? throw new NotFoundException(
                $"Không tìm thấy công thức có id '{request.RecipeId}'", RecipeErrorCodes.RecipeNotFound);

        // Quyết định S-11 & 2.14: Chỉ tác giả hoặc Admin mới có quyền thêm nguyên liệu
        authorizationHandler.EnsureCanModify(recipe);

        var maxOrderIndex = db.RecipeIngredients
            .Where(i => i.RecipeId == request.RecipeId)
            .Select(i => (int?)i.OrderIndex)
            .Max() ?? -1;
        var nextOrderIndex = maxOrderIndex + 1;

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
