using MediatR;

namespace CulinaryBlog.Application.Recipes.Commands.UnpublishRecipe;

public record UnpublishRecipeCommand(Guid Id) : IRequest;