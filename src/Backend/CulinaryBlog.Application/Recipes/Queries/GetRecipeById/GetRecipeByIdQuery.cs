using CulinaryBlog.Application.Recipes.Common;
using MediatR;

namespace CulinaryBlog.Application.Recipes.Queries.GetRecipeById;

public record GetRecipeByIdQuery(Guid Id) : IRequest<RecipeDto>;