using CulinaryBlog.Domain.Enums;
using MediatR;

namespace CulinaryBlog.Application.Recipes.Commands.UpdateRecipe;

public record UpdateRecipeCommand : IRequest
{
    public Guid Id { get; init; }
    public string Title { get; init; } = default!;
    public string Description { get; init; } = default!;
    public int PrepTimeMinutes { get; init; }
    public int CookTimeMinutes { get; init; }
    public int Servings { get; init; }
    public Difficulty Difficulty { get; init; }

    // B?t bu?c g?i l?i ?úng version ?ã nh?n t? GET tr??c ?ó (ADR-0002)
    public uint Version { get; init; }
}