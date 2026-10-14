using CulinaryBlog.Application.Abstractions;
using CulinaryBlog.Application.Common.Exceptions;
using FluentValidation;
using MediatR;
using ValidationException = CulinaryBlog.Application.Common.Exceptions.ValidationException;

namespace CulinaryBlog.Application.Features.Recipes;

public sealed record ReorderRecipeStepsRequest(List<Guid> StepIds);

public sealed record ReorderRecipeStepsCommand(Guid RecipeId, List<Guid> StepIds) : IRequest;

public sealed class ReorderRecipeStepsCommandValidator : AbstractValidator<ReorderRecipeStepsCommand>
{
    public ReorderRecipeStepsCommandValidator()
    {
        RuleFor(x => x.RecipeId)
            .NotEmpty();

        RuleFor(x => x.StepIds)
            .NotNull()
            .WithMessage("Danh sách các bước không được để trống.");

        RuleFor(x => x.StepIds)
            .Must(ids => ids == null || ids.Distinct().Count() == ids.Count)
            .WithErrorCode(RecipeErrorCodes.InvalidStepOrder)
            .WithMessage("Danh sách các bước không được chứa ID trùng lặp.");
    }
}

public sealed class ReorderRecipeStepsCommandHandler(
    IAppDbContext db,
    IRecipeAuthorizationHandler authorizationHandler,
    ICacheInvalidator cacheInvalidator) : IRequestHandler<ReorderRecipeStepsCommand>
{
    public async Task Handle(ReorderRecipeStepsCommand request, CancellationToken cancellationToken)
    {
        var recipe = db.Recipes.FirstOrDefault(r => r.Id == request.RecipeId)
            ?? throw new NotFoundException(
                $"Không tìm thấy công thức có id '{request.RecipeId}'", RecipeErrorCodes.RecipeNotFound);

        // Quyết định S-11 & 2.13: Chỉ tác giả hoặc Admin mới có quyền sửa đổi bước
        authorizationHandler.EnsureCanModify(recipe);

        var currentSteps = db.RecipeSteps
            .Where(s => s.RecipeId == request.RecipeId)
            .ToList();

        // Kiểm tra trùng lặp ID
        if (request.StepIds.Distinct().Count() != request.StepIds.Count)
        {
            throw new ValidationException(
                RecipeErrorCodes.InvalidStepOrder,
                "Danh sách các bước không được chứa ID trùng lặp.");
        }

        // Kiểm tra số lượng bước gửi lên phải khớp với số lượng bước hiện có của recipe
        if (request.StepIds.Count != currentSteps.Count)
        {
            throw new ValidationException(
                RecipeErrorCodes.InvalidStepOrder,
                "Số lượng bước không khớp với số lượng bước hiện có của công thức.");
        }

        var stepMap = currentSteps.ToDictionary(s => s.Id);

        // Kiểm tra xem tất cả stepIds có thuộc recipe không
        foreach (var stepId in request.StepIds)
        {
            if (!stepMap.TryGetValue(stepId, out var _))
            {
                throw new NotFoundException(
                    $"Không tìm thấy bước có id '{stepId}' trong công thức này.",
                    RecipeErrorCodes.StepNotFound);
            }
        }

        // Cập nhật StepNumber theo thứ tự mới (bắt đầu từ 1)
        for (var i = 0; i < request.StepIds.Count; i++)
        {
            var step = stepMap[request.StepIds[i]];
            step.StepNumber = i + 1;
        }

        // S-04 / C-28: Cập nhật Recipe.UpdatedAt để xmin của Recipe cha thay đổi theo
        recipe.UpdatedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(cancellationToken);

        // 2.20 & S-07: Invalidate cache cho recipe
        await cacheInvalidator.InvalidateAsync(["recipes", $"recipe:{recipe.Id}"], cancellationToken);
    }
}
