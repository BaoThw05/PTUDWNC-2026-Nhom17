using CulinaryBlog.Domain.Entities;
using CulinaryBlog.IntegrationTests.Infrastructure;
using CulinaryBlog.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace CulinaryBlog.IntegrationTests.RecipeImages;

[Collection(PostgresCollection.Name)]
public sealed class RecipeImageDatabaseTests(PostgresApiFactory factory)
{
    [Fact]
    public async Task PrimaryImageIndex_AllowsOtherImagesButRejectsSecondPrimaryForSameRecipe()
    {
        using var client = factory.CreateClient();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await using var transaction = await db.Database.BeginTransactionAsync();

        var authorId = (await db.Users.Select(user => user.Id).FirstAsync()).ToString();
        var firstRecipe = NewRecipe(authorId);
        var secondRecipe = NewRecipe(authorId);
        db.Recipes.AddRange(firstRecipe, secondRecipe);
        db.RecipeImages.AddRange(
            NewImage(firstRecipe.Id, true),
            NewImage(firstRecipe.Id, false),
            NewImage(firstRecipe.Id, false),
            NewImage(secondRecipe.Id, true));
        await db.SaveChangesAsync();

        db.RecipeImages.Add(NewImage(firstRecipe.Id, true));
        var exception = await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
        Assert.Equal(PostgresErrorCodes.UniqueViolation, Assert.IsType<PostgresException>(exception.InnerException).SqlState);
        Assert.Equal("IX_RecipeImages_RecipeId", ((PostgresException)exception.InnerException!).ConstraintName);
    }

    private static Recipe NewRecipe(string authorId) => new()
    {
        AuthorId = authorId,
        Title = "Test image index",
        Slug = $"test-image-index-{Guid.NewGuid():N}",
        Description = "Integration test recipe",
        PrepTimeMinutes = 10,
        CookTimeMinutes = 10,
        Servings = 2
    };

    private static RecipeImage NewImage(Guid recipeId, bool isPrimary) => new()
    {
        RecipeId = recipeId,
        OriginalKey = $"recipes/{recipeId}/{Guid.NewGuid():N}/original.jpg",
        IsPrimary = isPrimary
    };
}
