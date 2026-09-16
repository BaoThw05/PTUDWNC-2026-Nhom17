namespace CulinaryBlog.API.Endpoints.RecipeSearch;

internal sealed class RecipeSearchEndpoints : IEndpointModule
{
    public string Tag => "RecipeSearch";

    public string Description => "Danh sách công thức công khai và tìm kiếm (TV4)";

    public void MapEndpoints(IEndpointRouteBuilder api)
    {
        var recipeSearch = api.MapGroup("/recipes").WithTags(Tag);

        // TODO(TV4): map endpoint của module vào nhóm ở trên (FR-RCP-001, FR-SRCH-001 → 004).
    }
}
