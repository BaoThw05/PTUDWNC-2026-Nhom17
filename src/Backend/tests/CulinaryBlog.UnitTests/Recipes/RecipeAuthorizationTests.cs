using CulinaryBlog.Application.Abstractions;
using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Features.Recipes;
using CulinaryBlog.Domain.Entities;
using CulinaryBlog.Domain.Enums;

namespace CulinaryBlog.UnitTests.Recipes;

public class RecipeAuthorizationTests
{
    private static readonly Guid AuthorGuid = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid StrangerGuid = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private static readonly Guid AdminGuid = Guid.Parse("33333333-3333-3333-3333-333333333333");

    private readonly TestCurrentUser _currentUser = new();
    private readonly RecipeAuthorizationHandler _handler;

    public RecipeAuthorizationTests()
    {
        _handler = new RecipeAuthorizationHandler(_currentUser);
    }

    [Fact]
    public void EnsureCanModify_WhenUserNotAuthenticated_ThrowsUnauthorizedException()
    {
        _currentUser.UserId = null;
        var recipe = new Recipe { AuthorId = AuthorGuid.ToString() };

        var ex = Assert.Throws<UnauthorizedException>(() => _handler.EnsureCanModify(recipe));
        Assert.Equal("UNAUTHORIZED", ex.Code);
    }

    [Fact]
    public void EnsureCanModify_WhenUserIsNotAuthorAndNotAdmin_ThrowsForbiddenExceptionWithRecipeForbiddenCode()
    {
        _currentUser.UserId = StrangerGuid;
        _currentUser.Role = "User";
        var recipe = new Recipe { AuthorId = AuthorGuid.ToString() };

        var ex = Assert.Throws<ForbiddenException>(() => _handler.EnsureCanModify(recipe));
        Assert.Equal(RecipeErrorCodes.RecipeForbidden, ex.Code);
    }

    [Fact]
    public void EnsureCanModify_WhenUserIsAuthor_DoesNotThrow()
    {
        _currentUser.UserId = AuthorGuid;
        var recipe = new Recipe { AuthorId = AuthorGuid.ToString() };

        var exception = Record.Exception(() => _handler.EnsureCanModify(recipe));
        Assert.Null(exception);
    }

    [Fact]
    public void EnsureCanModify_WhenUserIsAdmin_DoesNotThrowEvenIfNotAuthor()
    {
        _currentUser.UserId = AdminGuid;
        _currentUser.Role = "Admin";
        var recipe = new Recipe { AuthorId = AuthorGuid.ToString() };

        var exception = Record.Exception(() => _handler.EnsureCanModify(recipe));
        Assert.Null(exception);
    }

    [Fact]
    public void EnsureCanView_WhenRecipePublishedAndNotDeleted_AllowsAnyone()
    {
        _currentUser.UserId = null; // Khách vãng lai
        var recipe = new Recipe
        {
            Id = Guid.NewGuid(),
            AuthorId = AuthorGuid.ToString(),
            Status = RecipeStatus.Published,
            IsDeleted = false
        };

        var exception = Record.Exception(() => _handler.EnsureCanView(recipe));
        Assert.Null(exception);
    }

    [Fact]
    public void EnsureCanView_WhenRecipeIsDraftAndUserIsStranger_ThrowsNotFoundException()
    {
        // Theo S-11: Draft của người khác phải trả về 404 RECIPE_NOT_FOUND để không làm lộ thông tin
        _currentUser.UserId = StrangerGuid;
        var recipe = new Recipe
        {
            Id = Guid.NewGuid(),
            AuthorId = AuthorGuid.ToString(),
            Status = RecipeStatus.Draft,
            IsDeleted = false
        };

        var ex = Assert.Throws<NotFoundException>(() => _handler.EnsureCanView(recipe));
        Assert.Equal(RecipeErrorCodes.RecipeNotFound, ex.Code);
    }

    [Fact]
    public void EnsureCanView_WhenRecipeIsDraftAndUserIsAuthor_AllowsViewing()
    {
        _currentUser.UserId = AuthorGuid;
        var recipe = new Recipe
        {
            Id = Guid.NewGuid(),
            AuthorId = AuthorGuid.ToString(),
            Status = RecipeStatus.Draft,
            IsDeleted = false
        };

        var exception = Record.Exception(() => _handler.EnsureCanView(recipe));
        Assert.Null(exception);
    }

    [Fact]
    public void EnsureCanView_WhenRecipeIsDraftAndUserIsAdmin_AllowsViewing()
    {
        _currentUser.UserId = AdminGuid;
        _currentUser.Role = "Admin";
        var recipe = new Recipe
        {
            Id = Guid.NewGuid(),
            AuthorId = AuthorGuid.ToString(),
            Status = RecipeStatus.Draft,
            IsDeleted = false
        };

        var exception = Record.Exception(() => _handler.EnsureCanView(recipe));
        Assert.Null(exception);
    }

    private sealed class TestCurrentUser : ICurrentUser
    {
        public Guid? UserId { get; set; }
        public string? IpAddress { get; set; } = "127.0.0.1";
        public string? Role { get; set; }
        public bool IsInRole(string role) => string.Equals(Role, role, StringComparison.OrdinalIgnoreCase);
    }
}
