using System.Text.Json;
using CulinaryBlog.API.Auth;
using CulinaryBlog.Application.Features.RecipeImages;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CulinaryBlog.API.Endpoints.RecipeImages;

internal sealed class RecipeImagesEndpoints : IEndpointModule
{
    public string Tag => "RecipeImages";

    public string Description => "Ảnh công thức, lưu trữ file, job resize (TV3)";

    public void MapEndpoints(IEndpointRouteBuilder api)
    {
        var recipeImages = api.MapGroup("/recipes/{recipeId:guid}/images")
            .WithTags(Tag)
            .RequireAuthorization(AuthPolicies.Author);

        recipeImages.MapGet("/", async (
            Guid recipeId,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var images = await mediator.Send(new GetRecipeImagesQuery(recipeId), cancellationToken);
            return Results.Ok(images);
        }).WithName("GetRecipeImages");

        recipeImages.MapPost("/", async (
            Guid recipeId,
            IFormFile file,
            [FromForm] string? altText,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            await using var content = file.OpenReadStream();
            var result = await mediator.Send(new UploadRecipeImageCommand(recipeId, content, file.ContentType, altText), cancellationToken);
            return Results.Created($"/api/v1/recipes/{recipeId}/images/{result.ImageId}", result);
        })
        .DisableAntiforgery()
        .WithName("UploadRecipeImage");

        recipeImages.MapPatch("/{imageId:guid}", async (
            Guid recipeId,
            Guid imageId,
            PatchRecipeImageRequest request,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var hasAltText = request.AltText.ValueKind != JsonValueKind.Undefined;
            if (hasAltText && request.AltText.ValueKind is not (JsonValueKind.String or JsonValueKind.Null))
                throw new BadHttpRequestException("altText phải là chuỗi hoặc null.");

            var altText = request.AltText.ValueKind == JsonValueKind.String
                ? request.AltText.GetString()
                : null;
            var result = await mediator.Send(new UpdateRecipeImageCommand(
                recipeId, imageId, hasAltText, altText, request.IsPrimary, request.OrderIndex), cancellationToken);
            return Results.Ok(result);
        }).WithName("UpdateRecipeImage");

        recipeImages.MapDelete("/{imageId:guid}", async (
            Guid recipeId,
            Guid imageId,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            await mediator.Send(new DeleteRecipeImageCommand(recipeId, imageId), cancellationToken);
            return Results.NoContent();
        }).WithName("DeleteRecipeImage");
    }
}

public sealed class PatchRecipeImageRequest
{
    public JsonElement AltText { get; init; }
    public bool? IsPrimary { get; init; }
    public int? OrderIndex { get; init; }
}
