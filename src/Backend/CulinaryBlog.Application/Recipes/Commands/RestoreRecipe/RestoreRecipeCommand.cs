using MediatR;

namespace CulinaryBlog.Application.Recipes.Commands.RestoreRecipe;

public record RestoreRecipeCommand(Guid Id) : IRequest;