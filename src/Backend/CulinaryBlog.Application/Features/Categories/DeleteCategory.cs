using CulinaryBlog.Application.Abstractions;
using CulinaryBlog.Application.Common.Exceptions;
using MediatR;

namespace CulinaryBlog.Application.Features.Categories;

public sealed record DeleteCategoryCommand(Guid Id) : IRequest;

public sealed class DeleteCategoryCommandHandler(IAppDbContext db) : IRequestHandler<DeleteCategoryCommand>
{
    public async Task Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = db.Categories.FirstOrDefault(item => item.Id == request.Id)
            ?? throw new NotFoundException(
                $"Không tìm thấy danh mục có id '{request.Id}'.",
                CategoryErrorCodes.CategoryNotFound);

        var recipeCount = db.RecipesIncludingDeleted.Count(recipe => recipe.CategoryId == category.Id);
        if (recipeCount > 0)
        {
            throw new ConflictException(
                $"Không thể xóa danh mục vì còn {recipeCount} công thức thuộc danh mục.",
                CategoryErrorCodes.CategoryDeleteHasRecipes);
        }

        db.Remove(category);
        await db.SaveChangesAsync(cancellationToken);
    }
}
