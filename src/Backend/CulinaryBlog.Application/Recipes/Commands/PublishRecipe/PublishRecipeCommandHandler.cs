using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CulinaryBlog.Application.Recipes.Commands.PublishRecipe;

public class PublishRecipeCommandHandler : IRequestHandler<PublishRecipeCommand>
{
    private readonly IApplicationDbContext _context;

    public PublishRecipeCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(PublishRecipeCommand request, CancellationToken cancellationToken)
    {
        var recipe = await _context.Recipes
            .Include(r => r.Steps)
            .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);

        if (recipe is null)
        {
            throw new NotFoundException("RECIPE_NOT_FOUND", $"Không tìm thấy công thức có id '{request.Id}'");
        }

        // Idempotent (ADR-0003): đã Published rồi thì gọi lại vẫn OK, không báo lỗi
        if (recipe.Status == RecipeStatus.Published)
        {
            return;
        }

        if (recipe.Steps.Count == 0)
        {
            throw new BusinessRuleException(
                "RECIPE_PUBLISH_INCOMPLETE",
                "Công thức cần có ít nhất 1 bước thực hiện mới có thể xuất bản");
        }

        recipe.Status = RecipeStatus.Published;

        // PublishedAt chỉ set ở lần đầu tiên, giữ nguyên cho các lần publish lại sau (ADR-0003)
        recipe.PublishedAt ??= DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
    }
}