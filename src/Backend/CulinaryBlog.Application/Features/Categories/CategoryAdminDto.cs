namespace CulinaryBlog.Application.Features.Categories;

public sealed record CategoryAdminDto(
    Guid Id,
    string Name,
    string Slug,
    string? Description,
    string? ImageUrl,
    int OrderIndex,
    int RecipeCount);

public static class CategoryAdminErrorCodes
{
    public const string CategoryNotFound = "CATEGORY_NOT_FOUND";
    public const string CategoryNameExists = "CATEGORY_NAME_EXISTS";
    public const string CategoryDeleteHasRecipes = "CATEGORY_DELETE_HAS_RECIPES";
}
