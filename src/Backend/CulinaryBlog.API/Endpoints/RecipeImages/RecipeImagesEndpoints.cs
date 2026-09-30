using CulinaryBlog.Application.Features.RecipeImages;
using MediatR;

namespace CulinaryBlog.API.Endpoints.RecipeImages;

internal sealed class RecipeImagesEndpoints : IEndpointModule
{
    public string Tag => "RecipeImages";

    public string Description => "Ảnh công thức, lưu trữ file, job resize (TV3)";

    public void MapEndpoints(IEndpointRouteBuilder api)
    {
        var recipeImages = api.MapGroup("/recipes/{recipeId:guid}/images").WithTags(Tag);

        recipeImages.MapPost("/", async (
            Guid recipeId,
            IFormFile file,
            string? altText,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            await using var content = file.OpenReadStream();
            var result = await mediator.Send(new UploadRecipeImageCommand(recipeId, content, altText), cancellationToken);
            return Results.Created($"/api/v1/recipes/{recipeId}/images/{result.ImageId}", result);
        })
        .WithName("UploadRecipeImage");
    }
}
