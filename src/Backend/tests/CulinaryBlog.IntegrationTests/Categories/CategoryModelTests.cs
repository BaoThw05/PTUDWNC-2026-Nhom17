using CulinaryBlog.Domain.Entities;
using CulinaryBlog.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

namespace CulinaryBlog.IntegrationTests.Categories;

public sealed class CategoryModelTests
{
    [Fact]
    public void Model_MapsCaseInsensitiveUniqueCategoryNameAndSlug()
    {
        using var db = CreateDbContext();
        var category = db.GetService<IDesignTimeModel>().Model.FindEntityType(typeof(Category));

        Assert.NotNull(category);
        Assert.Equal("citext", category.FindProperty(nameof(Category.Name))?.GetColumnType());
        Assert.Equal(100, category.FindProperty(nameof(Category.Name))?.GetMaxLength());
        Assert.Equal(120, category.FindProperty(nameof(Category.Slug))?.GetMaxLength());
        Assert.Contains(category.GetIndexes(), index =>
            index.IsUnique && index.Properties.Single().Name == nameof(Category.Name));
        Assert.Contains(category.GetIndexes(), index =>
            index.IsUnique && index.Properties.Single().Name == nameof(Category.Slug));
    }

    [Fact]
    public void Model_SeedsEightCategoriesAndRestrictsCategoryDeletion()
    {
        using var db = CreateDbContext();
        var model = db.GetService<IDesignTimeModel>().Model;
        var category = model.FindEntityType(typeof(Category));
        var recipe = model.FindEntityType(typeof(Recipe));

        Assert.NotNull(category);
        Assert.NotNull(recipe);
        Assert.Equal(8, category.GetSeedData().Count());
        Assert.Contains(category.GetSeedData(), seed => (string)seed[nameof(Category.Slug)] == "mon-chinh");
        Assert.Contains(recipe.GetForeignKeys(), foreignKey =>
            foreignKey.PrincipalEntityType == category
            && foreignKey.Properties.Single().Name == nameof(Recipe.CategoryId)
            && foreignKey.DeleteBehavior == DeleteBehavior.Restrict);
    }

    private static AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql("Host=localhost;Database=culinary_blog;Username=postgres;Password=postgres")
            .Options;

        return new AppDbContext(options);
    }
}
