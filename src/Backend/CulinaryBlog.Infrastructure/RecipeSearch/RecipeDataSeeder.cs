using Bogus;
using CulinaryBlog.Application.Features.Recipes;
using CulinaryBlog.Application.Abstractions;
using CulinaryBlog.Domain.Auth;
using CulinaryBlog.Domain.Entities;
using CulinaryBlog.Domain.Enums;
using CulinaryBlog.Infrastructure.Auth;
using CulinaryBlog.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CulinaryBlog.Infrastructure.RecipeSearch;

internal sealed class RecipeDataSeeder(
    AppDbContext db,
    IFileStorage fileStorage,
    UserManager<ApplicationUser> userManager,
    IOptions<AuthSeedOptions> authSeedOptions,
    TimeProvider timeProvider,
    ILogger<RecipeDataSeeder> logger) : IDataSeeder
{
    private static readonly string[] DishNames =
    [
        "Phở bò", "Phở gà", "Bún bò Huế", "Bún chả Hà Nội", "Bún riêu cua",
        "Bún thịt nướng", "Cơm tấm sườn", "Cơm gà Hội An", "Cá kho tộ", "Thịt kho trứng",
        "Gỏi cuốn tôm thịt", "Bánh mì thịt nướng", "Bánh xèo miền Tây", "Chả giò", "Canh chua cá",
        "Mì Quảng", "Hủ tiếu Nam Vang", "Bánh cuốn nóng", "Bò lúc lắc", "Gà kho gừng",
        "Rau muống xào tỏi", "Đậu hũ sốt cà", "Chè ba màu", "Chè đậu xanh", "Sữa chua nếp cẩm"
    ];

    private static readonly string[] Variations = ["truyền thống", "gia đình"];

    private static readonly string[] DescriptionOpenings =
    [
        "Món ăn đậm đà hương vị quê nhà",
        "Công thức dễ làm cho bữa cơm sum họp",
        "Món ngon thơm nóng với nguyên liệu quen thuộc",
        "Cách nấu tròn vị, phù hợp cho cả gia đình",
        "Một lựa chọn hấp dẫn cho thực đơn cuối tuần"
    ];

    private static readonly string[] DescriptionDetails =
    [
        "nêm nếm vừa ăn và dùng khi còn nóng",
        "chuẩn bị nguyên liệu tươi rồi sơ chế sạch",
        "có thể gia giảm gia vị theo khẩu vị",
        "thực hiện lần lượt từng bước để giữ trọn hương vị",
        "dùng kèm rau thơm và nước chấm phù hợp"
    ];

    public async Task SeedAsync(CancellationToken cancellationToken)
    {
        var authors = await EnsureSampleAuthorsAsync(cancellationToken);
        if (authors.Count < 5)
        {
            logger.LogWarning("Recipe seed needs five author accounts; configure Seed:Auth:AuthorPassword");
            return;
        }

        var categories = await db.Categories
            .OrderBy(category => category.OrderIndex)
            .ToListAsync(cancellationToken);
        if (categories.Count == 0)
        {
            logger.LogWarning("Recipe seed needs at least one category; category seed was not applied");
            return;
        }

        var faker = new Faker();
        var now = timeProvider.GetUtcNow();
        var existingRecipes = await db.Recipes.IgnoreQueryFilters()
            .Where(recipe => recipe.Slug.StartsWith("sample-"))
            .ToListAsync(cancellationToken);
        var recipesByIndex = existingRecipes
            .Select(recipe => (Recipe: recipe, Index: GetSampleIndex(recipe.Slug)))
            .Where(item => item.Index.HasValue)
            .ToDictionary(item => item.Index!.Value, item => item.Recipe);
        var recipes = new Recipe[50];
        var newRecipes = new List<Recipe>();

        for (var index = 0; index < recipes.Length; index++)
        {
            var category = categories[index % categories.Count];
            if (!recipesByIndex.TryGetValue(index + 1, out var recipe))
            {
                recipe = CreateRecipe(index, authors[index % authors.Count], category.Id, faker, now);
                newRecipes.Add(recipe);
            }
            else
            {
                recipe.CategoryId = category.Id;
            }

            recipes[index] = recipe;
        }

        await db.Recipes.AddRangeAsync(newRecipes, cancellationToken);

        var sampleRecipeIds = recipes.Select(recipe => recipe.Id).ToArray();
        var recipesWithPrimaryImage = await db.RecipeImages.IgnoreQueryFilters()
            .Where(image => sampleRecipeIds.Contains(image.RecipeId) && image.IsPrimary)
            .Select(image => image.RecipeId)
            .ToHashSetAsync(cancellationToken);
        var imageIndexes = Enumerable.Range(0, recipes.Length)
            .Where(index => index % 10 == 0 && recipes[index].Status == RecipeStatus.Published
                && !recipesWithPrimaryImage.Contains(recipes[index].Id))
            .ToArray();
        var availableArtwork = new HashSet<int>();

        foreach (var variant in imageIndexes.Select(index => index / 10 % 3).Distinct())
        {
            var key = SampleImageKey(variant);
            try
            {
                await using var stream = new MemoryStream(SampleRecipeArtwork.CreatePng(variant));
                await fileStorage.UploadAsync(stream, key, "image/png", cancellationToken);
                availableArtwork.Add(variant);
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                logger.LogWarning(exception, "Could not upload sample recipe artwork {StorageKey}", key);
            }
        }

        foreach (var index in imageIndexes)
        {
            var variant = index / 10 % 3;
            if (!availableArtwork.Contains(variant))
            {
                continue;
            }

            var recipe = recipes[index];
            db.RecipeImages.Add(new RecipeImage
            {
                Id = Guid.CreateVersion7(),
                RecipeId = recipe.Id,
                OriginalKey = SampleImageKey(variant),
                ThumbnailKey = SampleImageKey(variant),
                AltText = $"Món {recipe.Title}",
                IsPrimary = true,
                OrderIndex = 0,
                CreatedAt = now
            });
        }

        await db.SaveChangesAsync(cancellationToken);
        logger.LogInformation(
            "Ensured {RecipeCount} sample recipes for {AuthorCount} authors and {CategoryCount} categories",
            recipes.Length,
            authors.Count,
            categories.Count);
    }

    private async Task<IReadOnlyList<ApplicationUser>> EnsureSampleAuthorsAsync(CancellationToken cancellationToken)
    {
        var seedOptions = authSeedOptions.Value;
        var emails = seedOptions.AuthorEmails.Take(5).ToList();
        while (emails.Count < 5)
        {
            emails.Add($"author{emails.Count + 1}@culinaryblog.test");
        }

        var authors = new List<ApplicationUser>(5);
        foreach (var email in emails.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            var author = await userManager.FindByEmailAsync(email);
            if (author is null)
            {
                if (string.IsNullOrWhiteSpace(seedOptions.AuthorPassword))
                {
                    logger.LogWarning("Cannot seed author {Email} because Seed:Auth:AuthorPassword is not configured", email);
                    continue;
                }

                author = new ApplicationUser
                {
                    Id = Guid.CreateVersion7(),
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true,
                    FullName = $"Sample Author {authors.Count + 1}",
                    CreatedAt = timeProvider.GetUtcNow()
                };

                var createResult = await userManager.CreateAsync(author, seedOptions.AuthorPassword);
                EnsureSucceeded(createResult, email);
            }

            if (!await userManager.IsInRoleAsync(author, Roles.Author))
            {
                var roleResult = await userManager.AddToRoleAsync(author, Roles.Author);
                EnsureSucceeded(roleResult, email);
            }

            authors.Add(author);
        }

        return authors;
    }

    private static Recipe CreateRecipe(
        int index,
        ApplicationUser author,
        Guid categoryId,
        Faker faker,
        DateTimeOffset now)
    {
        var title = $"{DishNames[index / Variations.Length]} {Variations[index % Variations.Length]}";
        var createdAt = now.AddDays(-faker.Random.Int(1, 180)).AddMinutes(-index);
        var status = (index % 10) switch
        {
            < 7 => RecipeStatus.Published,
            < 9 => RecipeStatus.Draft,
            _ => RecipeStatus.Archived
        };

        var recipe = new Recipe
        {
            Id = Guid.CreateVersion7(),
            Title = title,
            Slug = $"sample-{index + 1:00}-{SlugHelper.GenerateSlug(title)}",
            Description = $"{faker.PickRandom(DescriptionOpenings)}; {faker.PickRandom(DescriptionDetails)}.",
            PrepTimeMinutes = faker.Random.Int(5, 60),
            CookTimeMinutes = faker.Random.Int(0, 150),
            Servings = faker.Random.Int(1, 8),
            Difficulty = (Difficulty)faker.Random.Int(0, 2),
            Status = status,
            PublishedAt = status == RecipeStatus.Published ? createdAt.AddHours(faker.Random.Int(1, 24)) : null,
            AuthorId = author.Id.ToString(),
            CategoryId = categoryId,
            IsDeleted = false,
            CreatedAt = createdAt,
            UpdatedAt = null
        };

        if (status == RecipeStatus.Published)
        {
            recipe.Steps.Add(new RecipeStep
            {
                Id = Guid.CreateVersion7(),
                RecipeId = recipe.Id,
                StepNumber = 1,
                Title = "Chuẩn bị và hoàn thành món ăn",
                Description = "Sơ chế nguyên liệu, nấu chín theo công thức và nêm nếm vừa ăn.",
                DurationMinutes = recipe.PrepTimeMinutes + recipe.CookTimeMinutes,
                CreatedAt = createdAt
            });
        }

        return recipe;
    }

    private static int? GetSampleIndex(string slug)
    {
        var parts = slug.Split('-', 3);
        return parts.Length >= 2 && parts[0] == "sample" && int.TryParse(parts[1], out var number)
            ? number
            : null;
    }

    private static string SampleImageKey(int variant) => $"seed/recipes/sample-dish-{variant + 1:00}.png";

    private static void EnsureSucceeded(IdentityResult result, string subject)
    {
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(
                $"Recipe data seeding for '{subject}' failed: {string.Join("; ", result.Errors.Select(error => error.Description))}");
        }
    }
}
