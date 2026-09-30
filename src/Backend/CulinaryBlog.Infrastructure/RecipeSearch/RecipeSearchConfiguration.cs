using CulinaryBlog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NpgsqlTypes;

namespace CulinaryBlog.Infrastructure.RecipeSearch;

internal sealed class RecipeSearchConfiguration : IEntityTypeConfiguration<Recipe>
{
    public void Configure(EntityTypeBuilder<Recipe> builder)
    {
        builder.Property<NpgsqlTsVector>("SearchVector")
            .HasColumnType("tsvector")
            .IsRequired()
            .ValueGeneratedOnAddOrUpdate();

        builder.Property<string>("SearchText")
            .HasColumnType("text")
            .IsRequired()
            .ValueGeneratedOnAddOrUpdate();

        builder.HasIndex("SearchVector")
            .HasDatabaseName("IX_Recipes_SearchVector")
            .HasMethod("gin");

        builder.HasIndex("SearchText")
            .HasDatabaseName("IX_Recipes_SearchText_Trgm")
            .HasMethod("gin")
            .HasOperators("gin_trgm_ops");

        builder.HasIndex("Status", "PublishedAt")
            .HasDatabaseName("IX_Recipes_Status_PublishedAt")
            .IsDescending(false, true);

        builder.HasIndex("CookTimeMinutes")
            .HasDatabaseName("IX_Recipes_CookTimeMinutes");
    }
}
