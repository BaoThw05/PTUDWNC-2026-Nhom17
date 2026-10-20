using CulinaryBlog.Application.Abstractions;
using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Domain.Enums;
using MediatR;

namespace CulinaryBlog.Application.Features.Recipes;

public sealed record UnpublishRecipeCommand(Guid Id) : IRequest;

public sealed class UnpublishRecipeCommandHandler(
    IAppDbContext db,
    IRecipeAuthorizationHandler authorizationHandler) : IRequestHandler<UnpublishRecipeCommand>
{
    public async Task Handle(UnpublishRecipeCommand request, CancellationToken cancellationToken)
    {
        var recipe = db.Recipes.FirstOrDefault(r => r.Id == request.Id)
            ?? throw new NotFoundException(
                $"Không tìm thấy công thức có id '{request.Id}'", RecipeErrorCodes.RecipeNotFound);

        // Quyết định S-11 & 2.11: Chỉ tác giả hoặc Admin mới có quyền unpublish
        authorizationHandler.EnsureCanModify(recipe);

        // Idempotent: Nếu đã là Draft thì trả về thành công
        if (recipe.Status == RecipeStatus.Draft)
        {
            return;
        }

        // Quyết định ADR-005: Chỉ công thức đang Published mới có thể Unpublish về Draft
        if (recipe.Status != RecipeStatus.Published)
        {
            throw new ValidationException(
                RecipeErrorCodes.InvalidStateTransition,
                "Chỉ có thể hủy xuất bản công thức đang ở trạng thái Published.");
        }

        recipe.Status = RecipeStatus.Draft;
        recipe.UpdatedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(cancellationToken);
    }
}
