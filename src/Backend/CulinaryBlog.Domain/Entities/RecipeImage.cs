using CulinaryBlog.Domain.Common;

namespace CulinaryBlog.Domain.Entities;

public class RecipeImage : BaseEntity
{
    public Guid RecipeId { get; set; }
    public string OriginalKey { get; set; } = default!;
    public string? MediumKey { get; set; }
    public string? ThumbnailKey { get; set; }
    public string? AltText { get; set; }
    public bool IsPrimary { get; set; }
    public int OrderIndex { get; set; }

    public Recipe Recipe { get; set; } = default!;
}
