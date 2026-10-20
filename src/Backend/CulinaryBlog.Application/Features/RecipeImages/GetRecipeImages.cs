using CulinaryBlog.Application.Abstractions;
using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Features.Recipes;
using MediatR;

namespace CulinaryBlog.Application.Features.RecipeImages;

public sealed record GetRecipeImagesQuery(Guid RecipeId) : IRequest<IReadOnlyList<RecipeImageDto>>;

public sealed class GetRecipeImagesQueryHandler(
    IAppDbContext db,
    IFileStorage storage,
    IRecipeAuthorizationHandler authorization)
    : IRequestHandler<GetRecipeImagesQuery, IReadOnlyList<RecipeImageDto>>
{
    public Task<IReadOnlyList<RecipeImageDto>> Handle(GetRecipeImagesQuery request, CancellationToken cancellationToken)
    {
        var recipe = db.Recipes.FirstOrDefault(item => item.Id == request.RecipeId)
            ?? throw new NotFoundException($"Không tìm thấy công thức có id '{request.RecipeId}'.", "RECIPE_NOT_FOUND");
        authorization.EnsureCanModify(recipe);

        IReadOnlyList<RecipeImageDto> images = db.RecipeImages
            .Where(item => item.RecipeId == request.RecipeId)
            .OrderBy(item => item.OrderIndex)
            .ThenBy(item => item.Id)
            .ToList()
            .Select(item => new RecipeImageDto(item.Id, storage.GetPublicUrl(item.OriginalKey),
                item.MediumKey is null ? null : storage.GetPublicUrl(item.MediumKey),
                item.ThumbnailKey is null ? null : storage.GetPublicUrl(item.ThumbnailKey),
                item.AltText, item.IsPrimary, item.OrderIndex))
            .ToList();
        return Task.FromResult(images);
    }
}
