using CulinaryBlog.Application.Abstractions;
using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Features.Recipes;
using MediatR;

namespace CulinaryBlog.Application.Features.RecipeImages;

public sealed record UpdateRecipeImageCommand(
    Guid RecipeId, Guid ImageId, bool HasAltText, string? AltText, bool? IsPrimary, int? OrderIndex)
    : IRequest<RecipeImageDto>;

public sealed class UpdateRecipeImageCommandHandler(
    IAppDbContext db,
    IFileStorage storage,
    IRecipeAuthorizationHandler authorization,
    ICacheInvalidator cache) : IRequestHandler<UpdateRecipeImageCommand, RecipeImageDto>
{
    public async Task<RecipeImageDto> Handle(UpdateRecipeImageCommand request, CancellationToken cancellationToken)
    {
        var recipe = db.Recipes.FirstOrDefault(item => item.Id == request.RecipeId)
            ?? throw new NotFoundException($"Không tìm thấy công thức có id '{request.RecipeId}'.", "RECIPE_NOT_FOUND");
        authorization.EnsureCanModify(recipe);

        var image = db.RecipeImages.FirstOrDefault(item => item.Id == request.ImageId && item.RecipeId == request.RecipeId)
            ?? throw new NotFoundException($"Không tìm thấy ảnh có id '{request.ImageId}'.", "IMAGE_NOT_FOUND");

        if (request.HasAltText && request.AltText?.Length > 500)
            throw new ValidationException("IMAGE_ALT_TEXT_INVALID", "Alt text không được vượt quá 500 ký tự.");
        if (request.OrderIndex is < 0)
            throw new ValidationException("IMAGE_ORDER_INVALID", "Thứ tự ảnh không được âm.");
        if (request.IsPrimary == false && image.IsPrimary)
            throw new ValidationException("IMAGE_PRIMARY_REQUIRED", "Hãy chọn ảnh chính khác trước khi bỏ ảnh chính hiện tại.");

        var oldPrimary = request.IsPrimary == true && !image.IsPrimary
            ? db.RecipeImages.FirstOrDefault(item => item.RecipeId == request.RecipeId && item.IsPrimary)
            : null;

        async Task SaveUpdateAsync()
        {
            if (oldPrimary is not null)
            {
                oldPrimary.IsPrimary = false;
                await db.SaveChangesAsync(cancellationToken);
            }

            if (request.HasAltText) image.AltText = request.AltText?.Trim();
            if (request.OrderIndex.HasValue) image.OrderIndex = request.OrderIndex.Value;
            if (request.IsPrimary == true) image.IsPrimary = true;
            recipe.UpdatedAt = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync(cancellationToken);
        }

        if (oldPrimary is not null)
            await db.ExecuteInTransactionAsync(SaveUpdateAsync, cancellationToken);
        else
            await SaveUpdateAsync();

        await cache.InvalidateAsync(["recipes"], cancellationToken);
        return new RecipeImageDto(image.Id, storage.GetPublicUrl(image.OriginalKey),
            image.MediumKey is null ? null : storage.GetPublicUrl(image.MediumKey),
            image.ThumbnailKey is null ? null : storage.GetPublicUrl(image.ThumbnailKey),
            image.AltText, image.IsPrimary, image.OrderIndex);
    }
}
