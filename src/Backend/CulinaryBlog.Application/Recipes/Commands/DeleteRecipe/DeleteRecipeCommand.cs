using MediatR;

namespace CulinaryBlog.Application.Recipes.Commands.DeleteRecipe;

public record DeleteRecipeCommand(Guid Id) : IRequest;