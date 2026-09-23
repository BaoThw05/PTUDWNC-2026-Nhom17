using CulinaryBlog.Application.Abstractions;
using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Domain.Entities;
using CulinaryBlog.Domain.Enums;

namespace CulinaryBlog.Application.Features.Recipes;

public interface IRecipeAuthorizationHandler
{
    void EnsureCanModify(Recipe recipe);
    void EnsureCanView(Recipe recipe);
    void EnsureCanView(RecipeStatus status, bool isDeleted, string authorId, Guid recipeId);
}

public sealed class RecipeAuthorizationHandler(ICurrentUser currentUser) : IRecipeAuthorizationHandler
{
    public void EnsureCanModify(Recipe recipe)
    {
        if (string.IsNullOrWhiteSpace(currentUser.UserId))
        {
            throw new UnauthorizedException("Bạn cần đăng nhập để thực hiện thao tác này.");
        }

        var isAuthor = string.Equals(currentUser.UserId, recipe.AuthorId, StringComparison.Ordinal);
        if (!isAuthor && !currentUser.IsAdmin)
        {
            throw new ForbiddenException(
                "Bạn không có quyền chỉnh sửa công thức này.",
                RecipeErrorCodes.RecipeForbidden);
        }
    }

    public void EnsureCanView(Recipe recipe)
    {
        EnsureCanView(recipe.Status, recipe.IsDeleted, recipe.AuthorId, recipe.Id);
    }

    public void EnsureCanView(RecipeStatus status, bool isDeleted, string authorId, Guid recipeId)
    {
        if (status == RecipeStatus.Published && !isDeleted)
        {
            return;
        }

        // Quyết định S-11: bài chưa xuất bản hoặc đã xóa mềm chỉ tác giả hoặc Admin mới được xem,
        // người khác truy cập nhận 404 để không làm lộ sự tồn tại của bài.
        var isAuthor = !string.IsNullOrWhiteSpace(currentUser.UserId) &&
                       string.Equals(currentUser.UserId, authorId, StringComparison.Ordinal);

        if (!isAuthor && !currentUser.IsAdmin)
        {
            throw new NotFoundException(
                $"Không tìm thấy công thức có id '{recipeId}'",
                RecipeErrorCodes.RecipeNotFound);
        }
    }
}
