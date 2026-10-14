using CulinaryBlog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CulinaryBlog.Infrastructure.RecipeImages;

public sealed class RecipeImageConfiguration : IEntityTypeConfiguration<RecipeImage>
{
    public void Configure(EntityTypeBuilder<RecipeImage> builder)
    {
        builder.Property(image => image.OriginalKey).IsRequired().HasMaxLength(500);
        builder.Property(image => image.MediumKey).HasMaxLength(500);
        builder.Property(image => image.ThumbnailKey).HasMaxLength(500);
        builder.Property(image => image.ProcessingError).HasMaxLength(500);
        builder.Property(image => image.AltText).HasMaxLength(500);
        builder.Property(image => image.OrderIndex).HasDefaultValue(0);

        builder.HasOne(image => image.Recipe)
            .WithMany(recipe => recipe.Images)
            .HasForeignKey(image => image.RecipeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasQueryFilter(image => !image.Recipe.IsDeleted);

        builder.HasIndex(image => new { image.RecipeId, image.OrderIndex });
        builder.HasIndex(image => image.RecipeId)
            .IsUnique()
            .HasFilter("\"IsPrimary\" = true");
    }
}
