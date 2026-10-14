using CulinaryBlog.Application.Abstractions;
using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Domain.Enums;
using MediatR;

namespace CulinaryBlog.Application.Features.Recipes;

public sealed record ArchiveRecipeCommand(Guid Id) : IRequest;

public sealed class ArchiveRecipeCommandHandler(
    IAppDbContext db,
    IRecipeAuthorizationHandler authorizationHandler) : IRequestHandler<ArchiveRecipeCommand>
{
    public async Task Handle(ArchiveRecipeCommand request, CancellationToken cancellationToken)
    {
        var recipe = db.Recipes.FirstOrDefault(r => r.Id == request.Id)
            ?? throw new NotFoundException(
                $"Không tìm thấy công thức có id '{request.Id}'", RecipeErrorCodes.RecipeNotFound);

        // Quyết định S-11 & 2.11: Chỉ tác giả hoặc Admin mới có quyền archive
        authorizationHandler.EnsureCanModify(recipe);

        // Quyết định S-11: Idempotent - nếu đã Archived thì coi như thành công
        if (recipe.Status == RecipeStatus.Archived)
        {
            return;
        }

        // Quyết định ADR-005: Draft hoặc Published mới được chuyển sang Archived
        if (recipe.Status != RecipeStatus.Draft && recipe.Status != RecipeStatus.Published)
        {
            throw new ValidationException(
                RecipeErrorCodes.InvalidStateTransition,
                "Chỉ có thể lưu trữ công thức đang ở trạng thái Draft hoặc Published.");
        }

        recipe.Status = RecipeStatus.Archived;
        recipe.UpdatedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(cancellationToken);
    }
}
