using CulinaryBlog.Application.Abstractions;
using CulinaryBlog.Application.Common.Exceptions;
using FluentValidation;
using MediatR;

namespace CulinaryBlog.Application.Features.Recipes;

public sealed record UpdateRecipeIngredientCommand : IRequest
{
    public Guid RecipeId { get; init; }
    public Guid IngredientId { get; init; }
    public string Name { get; init; } = default!;
    public decimal? Quantity { get; init; }
    public string? Unit { get; init; }
}

public sealed class UpdateRecipeIngredientCommandValidator : AbstractValidator<UpdateRecipeIngredientCommand>
{
    public UpdateRecipeIngredientCommandValidator()
    {
        RuleFor(x => x.RecipeId).NotEmpty();
        RuleFor(x => x.IngredientId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Quantity).GreaterThan(0).When(x => x.Quantity.HasValue);
    }
}

public sealed class UpdateRecipeIngredientCommandHandler(
    IAppDbContext db,
    IRecipeAuthorizationHandler authorizationHandler) : IRequestHandler<UpdateRecipeIngredientCommand>
{
    public async Task Handle(UpdateRecipeIngredientCommand request, CancellationToken cancellationToken)
    {
        var recipe = db.Recipes.FirstOrDefault(r => r.Id == request.RecipeId)
            ?? throw new NotFoundException(
                $"Không tìm thấy công thức có id '{request.RecipeId}'", RecipeErrorCodes.RecipeNotFound);

        // Quyết định S-11 & 2.14: Chỉ tác giả hoặc Admin mới có quyền sửa nguyên liệu
        authorizationHandler.EnsureCanModify(recipe);

        var ingredient = db.RecipeIngredients
            .FirstOrDefault(i => i.Id == request.IngredientId && i.RecipeId == request.RecipeId)
            ?? throw new NotFoundException(
                $"Không tìm thấy nguyên liệu có id '{request.IngredientId}'", RecipeErrorCodes.IngredientNotFound);

        ingredient.Name = request.Name;
        ingredient.Quantity = request.Quantity;
        ingredient.Unit = request.Unit;

        // S-04 / C-28: Cập nhật Recipe.UpdatedAt để xmin của Recipe cha thay đổi theo
        recipe.UpdatedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(cancellationToken);
    }
}
