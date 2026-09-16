namespace CulinaryBlog.API.Endpoints.Recipes;

internal sealed class RecipesEndpoints : IEndpointModule
{
    public string Tag => "Recipes";

    public string Description => "Tạo, sửa, publish, archive, thùng rác công thức; bước và nguyên liệu (TV2)";

    public void MapEndpoints(IEndpointRouteBuilder api)
    {
        var recipes = api.MapGroup("/recipes").WithTags(Tag);
        var me = api.MapGroup("/me").WithTags(Tag);

        // TODO(TV2): map endpoint của module vào nhóm ở trên (FR-RCP-002 → 007, 009, 010).
    }
}
