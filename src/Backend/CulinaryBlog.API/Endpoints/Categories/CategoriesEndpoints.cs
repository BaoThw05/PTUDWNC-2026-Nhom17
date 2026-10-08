namespace CulinaryBlog.API.Endpoints.Categories;

internal sealed class CategoriesEndpoints : IEndpointModule
{
    public string Tag => "Categories";

    public string Description => "Danh mục công thức (TV3)";

    public void MapEndpoints(IEndpointRouteBuilder api)
    {
        var categories = api.MapGroup("/categories").WithTags(Tag);

        // TODO(TV3): map endpoint của module vào nhóm ở trên (FR-CAT-001 → 005).
    }
}
