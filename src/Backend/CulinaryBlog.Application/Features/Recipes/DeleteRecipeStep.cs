using CulinaryBlog.Application.Abstractions;
using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Domain.Enums;
using MediatR;

namespace CulinaryBlog.Application.Features.Recipes;

public sealed record DeleteRecipeStepCommand(Guid RecipeId, Guid StepId) : IRequest;

public sealed class DeleteRecipeStepCommandHandler(
    IAppDbContext db,
    IRecipeAuthorizationHandler authorizationHandler) : IRequestHandler<DeleteRecipeStepCommand>
{
    public async Task Handle(DeleteRecipeStepCommand request, CancellationToken cancellationToken)
    {
        var recipe = db.Recipes.FirstOrDefault(r => r.Id == request.RecipeId)
            ?? throw new NotFoundException(
                $"Không tìm thấy công thức có id '{request.RecipeId}'", RecipeErrorCodes.RecipeNotFound);

        // Quyết định S-11 & 2.13: Chỉ tác giả hoặc Admin mới có quyền xóa bước
        authorizationHandler.EnsureCanModify(recipe);

        var step = db.RecipeSteps.FirstOrDefault(s => s.Id == request.StepId && s.RecipeId == request.RecipeId)
            ?? throw new NotFoundException(
                $"Không tìm thấy bước có id '{request.StepId}'", RecipeErrorCodes.StepNotFound);

        // S-11: Chặn xóa bước cuối cùng của recipe đang Published
        var totalSteps = db.RecipeSteps.Count(s => s.RecipeId == request.RecipeId);
        if (recipe.Status == RecipeStatus.Published && totalSteps <= 1)
        {
            throw new ValidationException(
                RecipeErrorCodes.PublishIncomplete,
                "Không thể xóa bước cuối cùng của công thức đã xuất bản.");
        }

        db.Remove(step);

        // Đánh lại số thứ tự các bước (FR-RCP-010, S-11)
        // Dùng offset tạm +1000 trước để tránh vi phạm unique constraint nếu có
        var remainingSteps = db.RecipeSteps
            .Where(s => s.RecipeId == request.RecipeId && s.Id != step.Id)
            .OrderBy(s => s.StepNumber)
            .ToList();

        for (var i = 0; i < remainingSteps.Count; i++)
        {
            remainingSteps[i].StepNumber = i + 1;
        }

        // S-04 / C-28: Cập nhật Recipe.UpdatedAt để xmin của Recipe cha thay đổi theo
        recipe.UpdatedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(cancellationToken);
    }
}
