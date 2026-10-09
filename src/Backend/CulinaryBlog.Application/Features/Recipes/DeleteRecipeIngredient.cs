using CulinaryBlog.Application.Abstractions;
using CulinaryBlog.Application.Common.Exceptions;
using MediatR;

namespace CulinaryBlog.Application.Features.Recipes;

public sealed record DeleteRecipeIngredientCommand(Guid RecipeId, Guid IngredientId) : IRequest;

public sealed class DeleteRecipeIngredientCommandHandler(
    IAppDbContext db,
    IRecipeAuthorizationHandler authorizationHandler) : IRequestHandler<DeleteRecipeIngredientCommand>
{
    public async Task Handle(DeleteRecipeIngredientCommand request, CancellationToken cancellationToken)
    {
        var recipe = db.Recipes.FirstOrDefault(r => r.Id == request.RecipeId)
            ?? throw new NotFoundException(
                $"Không tìm thấy công thức có id '{request.RecipeId}'", RecipeErrorCodes.RecipeNotFound);

        // Quyết định S-11 & 2.14: Chỉ tác giả hoặc Admin mới có quyền xóa nguyên liệu
        authorizationHandler.EnsureCanModify(recipe);

        var ingredient = db.RecipeIngredients
            .FirstOrDefault(i => i.Id == request.IngredientId && i.RecipeId == request.RecipeId)
            ?? throw new NotFoundException(
                $"Không tìm thấy nguyên liệu có id '{request.IngredientId}'", RecipeErrorCodes.IngredientNotFound);

        db.Remove(ingredient);

        // Tự đánh lại chỉ mục cho các nguyên liệu còn lại
        var remaining = db.RecipeIngredients
            .Where(i => i.RecipeId == request.RecipeId && i.Id != ingredient.Id)
            .OrderBy(i => i.OrderIndex)
            .ToList();

        for (var i = 0; i < remaining.Count; i++)
        {
            remaining[i].OrderIndex = i;
        }

        // S-04 / C-28: Cập nhật Recipe.UpdatedAt để xmin của Recipe cha thay đổi theo
        recipe.UpdatedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(cancellationToken);
    }
}
