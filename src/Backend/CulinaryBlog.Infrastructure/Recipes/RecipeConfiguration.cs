using CulinaryBlog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CulinaryBlog.Infrastructure.Recipes;

public sealed class RecipeConfiguration : IEntityTypeConfiguration<Recipe>
{
    public void Configure(EntityTypeBuilder<Recipe> builder)
    {
        builder.Property(r => r.Title).IsRequired().HasMaxLength(200);
        builder.Property(r => r.Description).IsRequired().HasMaxLength(2000);
        builder.Property(r => r.Slug).IsRequired().HasMaxLength(220);

        builder.HasIndex(r => r.Slug)
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false");

        builder.HasQueryFilter(r => !r.IsDeleted);

        builder.OwnsOne(r => r.Nutrition, n =>
        {
            n.Property(x => x.Calories).HasColumnName("Nutrition_Calories");
            n.Property(x => x.ProteinGrams).HasColumnName("Nutrition_ProteinGrams");
            n.Property(x => x.FatGrams).HasColumnName("Nutrition_FatGrams");
            n.Property(x => x.CarbsGrams).HasColumnName("Nutrition_CarbsGrams");
            n.Property(x => x.FiberGrams).HasColumnName("Nutrition_FiberGrams");
            n.Property(x => x.SugarGrams).HasColumnName("Nutrition_SugarGrams");
        });

        builder.HasMany(r => r.Steps)
            .WithOne(s => s.Recipe)
            .HasForeignKey(s => s.RecipeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(r => r.Ingredients)
            .WithOne(i => i.Recipe)
            .HasForeignKey(i => i.RecipeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class RecipeStepConfiguration : IEntityTypeConfiguration<RecipeStep>
{
    public void Configure(EntityTypeBuilder<RecipeStep> builder)
    {
        builder.Property(s => s.Description).IsRequired().HasMaxLength(2000);
        builder.Property(s => s.Title).HasMaxLength(200);
        builder.HasQueryFilter(s => !s.Recipe.IsDeleted);
    }
}

public sealed class RecipeIngredientConfiguration : IEntityTypeConfiguration<RecipeIngredient>
{
    public void Configure(EntityTypeBuilder<RecipeIngredient> builder)
    {
        builder.Property(i => i.Name).IsRequired().HasMaxLength(200);
        builder.HasQueryFilter(i => !i.Recipe.IsDeleted);
    }
}
