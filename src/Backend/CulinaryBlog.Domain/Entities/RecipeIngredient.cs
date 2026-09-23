using CulinaryBlog.Domain.Common;

namespace CulinaryBlog.Domain.Entities;

public class RecipeIngredient : BaseEntity
{
    public Guid RecipeId { get; set; }
    public Recipe Recipe { get; set; } = default!;

    public string Name { get; set; } = default!;
    public decimal? Quantity { get; set; }
    public string? Unit { get; set; }
    public int OrderIndex { get; set; }
}