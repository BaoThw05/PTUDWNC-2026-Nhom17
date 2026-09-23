using CulinaryBlog.Application.Abstractions;
using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Features.Recipes;
using CulinaryBlog.Domain.Entities;
using CulinaryBlog.Domain.Enums;

namespace CulinaryBlog.UnitTests.Recipes;

public class RecipeAuthorizationTests
{
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
        var recipe = new Recipe { AuthorId = "author-1" };

        var ex = Assert.Throws<UnauthorizedException>(() => _handler.EnsureCanModify(recipe));
        Assert.Equal("UNAUTHORIZED", ex.Code);
    }

    [Fact]
    public void EnsureCanModify_WhenUserIsNotAuthorAndNotAdmin_ThrowsForbiddenExceptionWithRecipeForbiddenCode()
    {
        _currentUser.UserId = "stranger-user";
        _currentUser.Role = "User";
        var recipe = new Recipe { AuthorId = "author-1" };

        var ex = Assert.Throws<ForbiddenException>(() => _handler.EnsureCanModify(recipe));
        Assert.Equal(RecipeErrorCodes.RecipeForbidden, ex.Code);
    }

    [Fact]
    public void EnsureCanModify_WhenUserIsAuthor_DoesNotThrow()
    {
        _currentUser.UserId = "author-1";
        var recipe = new Recipe { AuthorId = "author-1" };

        var exception = Record.Exception(() => _handler.EnsureCanModify(recipe));
        Assert.Null(exception);
    }

    [Fact]
    public void EnsureCanModify_WhenUserIsAdmin_DoesNotThrowEvenIfNotAuthor()
    {
        _currentUser.UserId = "admin-user";
        _currentUser.Role = "Admin";
        var recipe = new Recipe { AuthorId = "author-1" };

        var exception = Record.Exception(() => _handler.EnsureCanModify(recipe));
        Assert.Null(exception);
    }

    [Fact]
    public void EnsureCanView_WhenRecipePublishedAndNotDeleted_AllowsAnyone()
    {
        _currentUser.UserId = null; // Khach vang lai
        var recipe = new Recipe
        {
            Id = Guid.NewGuid(),
            AuthorId = "author-1",
            Status = RecipeStatus.Published,
            IsDeleted = false
        };

        var exception = Record.Exception(() => _handler.EnsureCanView(recipe));
        Assert.Null(exception);
    }

    [Fact]
    public void EnsureCanView_WhenRecipeIsDraftAndUserIsStranger_ThrowsNotFoundException()
    {
        // Theo S-11: Draft cua nguoi khac phai tra ve 404 RECIPE_NOT_FOUND de khong lam lo thong tin
        _currentUser.UserId = "stranger-user";
        var recipe = new Recipe
        {
            Id = Guid.NewGuid(),
            AuthorId = "author-1",
            Status = RecipeStatus.Draft,
            IsDeleted = false
        };

        var ex = Assert.Throws<NotFoundException>(() => _handler.EnsureCanView(recipe));
        Assert.Equal(RecipeErrorCodes.RecipeNotFound, ex.Code);
    }

    [Fact]
    public void EnsureCanView_WhenRecipeIsDraftAndUserIsAuthor_AllowsViewing()
    {
        _currentUser.UserId = "author-1";
        var recipe = new Recipe
        {
            Id = Guid.NewGuid(),
            AuthorId = "author-1",
            Status = RecipeStatus.Draft,
            IsDeleted = false
        };

        var exception = Record.Exception(() => _handler.EnsureCanView(recipe));
        Assert.Null(exception);
    }

    [Fact]
    public void EnsureCanView_WhenRecipeIsDraftAndUserIsAdmin_AllowsViewing()
    {
        _currentUser.UserId = "admin-1";
        _currentUser.Role = "Admin";
        var recipe = new Recipe
        {
            Id = Guid.NewGuid(),
            AuthorId = "author-1",
            Status = RecipeStatus.Draft,
            IsDeleted = false
        };

        var exception = Record.Exception(() => _handler.EnsureCanView(recipe));
        Assert.Null(exception);
    }

    private sealed class TestCurrentUser : ICurrentUser
    {
        public string? UserId { get; set; }
        public string? Role { get; set; }
        public bool IsAuthenticated => !string.IsNullOrWhiteSpace(UserId);
        public bool IsAdmin => string.Equals(Role, "Admin", StringComparison.OrdinalIgnoreCase);
    }
}
