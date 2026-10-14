using CulinaryBlog.Application.Abstractions;
using CulinaryBlog.Application.Common.Exceptions;
using FluentValidation;
using MediatR;

namespace CulinaryBlog.Application.Features.Recipes;

public sealed record UpdateRecipeStepCommand : IRequest
{
    public Guid RecipeId { get; init; }
    public Guid StepId { get; init; }
    public string? Title { get; init; }
    public string Description { get; init; } = default!;
    public int DurationMinutes { get; init; }
}

public sealed class UpdateRecipeStepCommandValidator : AbstractValidator<UpdateRecipeStepCommand>
{
    public UpdateRecipeStepCommandValidator()
    {
        RuleFor(x => x.RecipeId).NotEmpty();
        RuleFor(x => x.StepId).NotEmpty();
        RuleFor(x => x.Description).NotEmpty().MaximumLength(2000);
        RuleFor(x => x.Title).MaximumLength(200);
        RuleFor(x => x.DurationMinutes).GreaterThanOrEqualTo(0);
    }
}

public sealed class UpdateRecipeStepCommandHandler(
    IAppDbContext db,
    IRecipeAuthorizationHandler authorizationHandler) : IRequestHandler<UpdateRecipeStepCommand>
{
    public async Task Handle(UpdateRecipeStepCommand request, CancellationToken cancellationToken)
    {
        var recipe = db.Recipes.FirstOrDefault(r => r.Id == request.RecipeId)
            ?? throw new NotFoundException(
                $"Không tìm thấy công thức có id '{request.RecipeId}'", RecipeErrorCodes.RecipeNotFound);

        // Quyết định S-11 & 2.13: Chỉ tác giả hoặc Admin mới có quyền sửa bước
        authorizationHandler.EnsureCanModify(recipe);

        var step = db.RecipeSteps.FirstOrDefault(s => s.Id == request.StepId && s.RecipeId == request.RecipeId)
            ?? throw new NotFoundException(
                $"Không tìm thấy bước có id '{request.StepId}'", RecipeErrorCodes.StepNotFound);

        step.Title = request.Title;
        step.Description = request.Description;
        step.DurationMinutes = request.DurationMinutes;

        // S-04 / C-28: Cập nhật Recipe.UpdatedAt để xmin của Recipe cha thay đổi theo
        recipe.UpdatedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(cancellationToken);
    }
}
