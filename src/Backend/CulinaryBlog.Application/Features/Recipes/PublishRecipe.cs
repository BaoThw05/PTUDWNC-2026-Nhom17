using CulinaryBlog.Application.Abstractions;
using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Domain.Enums;
using MediatR;

namespace CulinaryBlog.Application.Features.Recipes;

public sealed record PublishRecipeCommand(Guid Id) : IRequest;

public sealed class PublishRecipeCommandHandler(
    IAppDbContext db,
    IRecipeAuthorizationHandler authorizationHandler) : IRequestHandler<PublishRecipeCommand>
{
    public async Task Handle(PublishRecipeCommand request, CancellationToken cancellationToken)
    {
        var recipe = db.Recipes.FirstOrDefault(r => r.Id == request.Id)
            ?? throw new NotFoundException(
                $"Không tìm thấy công thức có id '{request.Id}'", RecipeErrorCodes.RecipeNotFound);

        // Quyết định S-11 & 2.11: Chỉ tác giả hoặc Admin mới có quyền publish
        authorizationHandler.EnsureCanModify(recipe);

        // Quyết định S-11: Idempotent - nếu đã Published thì coi như thành công
        if (recipe.Status == RecipeStatus.Published)
        {
            return;
        }

        // Quyết định ADR-005: Archived chỉ có thể chuyển về Draft, không thể publish trực tiếp
        if (recipe.Status == RecipeStatus.Archived)
        {
            throw new ValidationException(
                RecipeErrorCodes.InvalidStateTransition,
                "Không thể xuất bản trực tiếp công thức đang bị lưu trữ. Vui lòng bỏ lưu trữ về bản nháp trước.");
        }

        // Quyết định S-11: Bắt buộc có ít nhất 1 bước thực hiện
        var stepCount = db.RecipeSteps.Count(s => s.RecipeId == recipe.Id);
        if (stepCount == 0)
        {
            throw new ValidationException(
                RecipeErrorCodes.PublishIncomplete,
                "Công thức cần có ít nhất 1 bước thực hiện mới có thể xuất bản.");
        }

        recipe.Status = RecipeStatus.Published;
        recipe.PublishedAt ??= DateTimeOffset.UtcNow;
        recipe.UpdatedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(cancellationToken);
    }
}
