using CulinaryBlog.Application.Abstractions;
using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Domain.Entities;
using FluentValidation;
using MediatR;

namespace CulinaryBlog.Application.Features.Recipes;

public sealed record AddRecipeStepCommand : IRequest<Guid>
{
    public Guid RecipeId { get; init; }
    public string? Title { get; init; }
    public string Description { get; init; } = default!;
    public int DurationMinutes { get; init; }
}

public sealed class AddRecipeStepCommandValidator : AbstractValidator<AddRecipeStepCommand>
{
    public AddRecipeStepCommandValidator()
    {
        RuleFor(x => x.RecipeId).NotEmpty();
        RuleFor(x => x.Description).NotEmpty().MaximumLength(2000);
        RuleFor(x => x.Title).MaximumLength(200);
        RuleFor(x => x.DurationMinutes).GreaterThanOrEqualTo(0);
    }
}

public sealed class AddRecipeStepCommandHandler(
    IAppDbContext db,
    IRecipeAuthorizationHandler authorizationHandler) : IRequestHandler<AddRecipeStepCommand, Guid>
{
    public async Task<Guid> Handle(AddRecipeStepCommand request, CancellationToken cancellationToken)
    {
        var recipe = db.Recipes.FirstOrDefault(r => r.Id == request.RecipeId)
            ?? throw new NotFoundException(
                $"Không tìm thấy công thức có id '{request.RecipeId}'", RecipeErrorCodes.RecipeNotFound);

        // Quyết định S-11 & 2.13: Chỉ tác giả hoặc Admin mới có quyền thêm bước
        authorizationHandler.EnsureCanModify(recipe);

        // Tự đánh số bước (FR-RCP-010): số thứ tự tiếp theo = max(StepNumber) + 1
        var maxNumber = db.RecipeSteps
            .Where(s => s.RecipeId == request.RecipeId)
            .Select(s => (int?)s.StepNumber)
            .Max() ?? 0;
        var nextStepNumber = maxNumber + 1;

        var step = new RecipeStep
        {
            RecipeId = request.RecipeId,
            StepNumber = nextStepNumber,
            Title = request.Title,
            Description = request.Description,
            DurationMinutes = request.DurationMinutes
        };

        db.Add(step);

        // S-04 / C-28: Cập nhật Recipe.UpdatedAt để xmin của Recipe cha thay đổi theo
        recipe.UpdatedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(cancellationToken);

        return step.Id;
    }
}
