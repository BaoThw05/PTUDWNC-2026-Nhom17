namespace CulinaryBlog.Application.Features.Categories;

public sealed record CategoryDto(
    Guid Id,
    string Name,
    string Slug,
    string? Description,
    string? ImageUrl,
    int OrderIndex,
    int RecipeCount);

public static class CategoryErrorCodes
{
    public const string CategoryNotFound = "CATEGORY_NOT_FOUND";
    public const string CategoryNameExists = "CATEGORY_NAME_EXISTS";
    public const string CategoryDeleteHasRecipes = "CATEGORY_DELETE_HAS_RECIPES";
}
