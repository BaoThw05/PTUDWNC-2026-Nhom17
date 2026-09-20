using CulinaryBlog.Domain.Enums;
using MediatR;

namespace CulinaryBlog.Application.Recipes.Commands.CreateRecipe;

public record CreateRecipeCommand : IRequest<Guid>
{
    public string Title { get; init; } = default!;
    public string Description { get; init; } = default!;
    public int PrepTimeMinutes { get; init; }
    public int CookTimeMinutes { get; init; }
    public int Servings { get; init; }
    public Difficulty Difficulty { get; init; }
    public string AuthorId { get; init; } = default!;
}