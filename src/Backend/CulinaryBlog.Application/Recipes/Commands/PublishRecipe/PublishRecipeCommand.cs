using MediatR;

namespace CulinaryBlog.Application.Recipes.Commands.PublishRecipe;

public record PublishRecipeCommand(Guid Id) : IRequest;