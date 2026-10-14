using CulinaryBlog.Application.Abstractions;
using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Domain.Enums;
using MediatR;

namespace CulinaryBlog.Application.Features.Recipes;

public sealed record UnarchiveRecipeCommand(Guid Id) : IRequest;

public sealed class UnarchiveRecipeCommandHandler(
    IAppDbContext db,
    IRecipeAuthorizationHandler authorizationHandler) : IRequestHandler<UnarchiveRecipeCommand>
{
    public async Task Handle(UnarchiveRecipeCommand request, CancellationToken cancellationToken)
    {
        var recipe = db.Recipes.FirstOrDefault(r => r.Id == request.Id)
            ?? throw new NotFoundException(
                $"Không tìm thấy công thức có id '{request.Id}'", RecipeErrorCodes.RecipeNotFound);

        // Quyết định S-11 & 2.11: Chỉ tác giả hoặc Admin mới có quyền unarchive
        authorizationHandler.EnsureCanModify(recipe);

        // Quyết định S-11: Idempotent - nếu đã là Draft thì coi như thành công
        if (recipe.Status == RecipeStatus.Draft)
        {
            return;
        }

        // Quyết định ADR-005: Chỉ công thức đang Archived mới được chuyển về Draft
        if (recipe.Status != RecipeStatus.Archived)
        {
            throw new ValidationException(
                RecipeErrorCodes.InvalidStateTransition,
                "Chỉ có thể bỏ lưu trữ công thức đang ở trạng thái Archived.");
        }

        recipe.Status = RecipeStatus.Draft;
        recipe.UpdatedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(cancellationToken);
    }
}
