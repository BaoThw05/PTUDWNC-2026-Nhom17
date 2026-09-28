namespace CulinaryBlog.Application.Features.Recipes;

public static class RecipeErrorCodes
{
    public const string RecipeNotFound = "RECIPE_NOT_FOUND";
    public const string RecipeForbidden = "RECIPE_FORBIDDEN";
    public const string RecipeConcurrencyConflict = "RECIPE_CONCURRENCY_CONFLICT";
    public const string PublishIncomplete = "RECIPE_PUBLISH_INCOMPLETE";
    public const string CategoryInvalid = "RECIPE_CATEGORY_INVALID";
    public const string InvalidStateTransition = "RECIPE_INVALID_STATE_TRANSITION";
    public const string StepNotFound = "STEP_NOT_FOUND";
    public const string IngredientNotFound = "INGREDIENT_NOT_FOUND";
}
