using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CulinaryBlog.Application.Recipes.Commands.RestoreRecipe;

public class RestoreRecipeCommandHandler : IRequestHandler<RestoreRecipeCommand>
{
    private readonly IApplicationDbContext _context;

    public RestoreRecipeCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(RestoreRecipeCommand request, CancellationToken cancellationToken)
    {
        // IgnoreQueryFilters() vì Global Query Filter mặc định ẩn recipe đã xóa mềm -
        // phải bỏ qua filter thì mới tìm thấy recipe đang nằm trong thùng rác
        var recipe = await _context.Recipes
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);

        if (recipe is null || !recipe.IsDeleted)
        {
            throw new NotFoundException("RECIPE_NOT_FOUND", $"Không tìm thấy công thức đã xóa có id '{request.Id}'");
        }

        // ADR-0001: nếu slug đã bị recipe khác chiếm trong lúc chờ khôi phục, tự thêm hậu tố
        var slugTaken = await _context.Recipes
            .AnyAsync(r => r.Slug == recipe.Slug && r.Id != recipe.Id, cancellationToken);

        if (slugTaken)
        {
            recipe.Slug = $"{recipe.Slug}-{Guid.NewGuid().ToString()[..6]}";
        }

        recipe.IsDeleted = false;
        recipe.DeletedAt = null;

        await _context.SaveChangesAsync(cancellationToken);
    }
}