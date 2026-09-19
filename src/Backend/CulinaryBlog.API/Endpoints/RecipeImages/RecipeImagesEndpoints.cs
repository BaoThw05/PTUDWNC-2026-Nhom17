namespace CulinaryBlog.API.Endpoints.RecipeImages;

internal sealed class RecipeImagesEndpoints : IEndpointModule
{
    public string Tag => "RecipeImages";

    public string Description => "Ảnh công thức, lưu trữ file, job resize (TV3)";

    public void MapEndpoints(IEndpointRouteBuilder api)
    {
        var recipeImages = api.MapGroup("/recipes/{recipeId:guid}/images").WithTags(Tag);

        // TODO(TV3): map endpoint của module vào nhóm ở trên (FR-RCP-008, FR-FILE, FR-JOB-002).
    }
}
