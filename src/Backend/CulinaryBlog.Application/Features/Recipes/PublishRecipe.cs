using CulinaryBlog.Application.Abstractions;
using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Domain.Enums;
using MediatR;

namespace CulinaryBlog.Application.Features.Recipes;

public sealed record PublishRecipeCommand(Guid Id) : IRequest;

public sealed class PublishRecipeCommandHandler(IAppDbContext db) : IRequestHandler<PublishRecipeCommand>
{
    public async Task Handle(PublishRecipeCommand request, CancellationToken cancellationToken)
    {
        var recipe = db.Recipes.FirstOrDefault(r => r.Id == request.Id)
            ?? throw new NotFoundException(
                $"Không tìm thấy công thức có id '{request.Id}'", RecipeErrorCodes.RecipeNotFound);

        if (recipe.Status == RecipeStatus.Published)
        {
            return; // idempotent (S-11)
        }

        var stepCount = db.RecipeSteps.Count(s => s.RecipeId == recipe.Id);

        if (stepCount == 0)
        {
            throw new ValidationException(
                RecipeErrorCodes.PublishIncomplete,
                "Công thức cần có ít nhất 1 bước thực hiện mới có thể xuất bản");
        }

        recipe.Status = RecipeStatus.Published;
        recipe.PublishedAt ??= DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(cancellationToken);
    }
}
