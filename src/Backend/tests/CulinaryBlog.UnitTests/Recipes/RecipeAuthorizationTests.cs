using CulinaryBlog.Application.Abstractions;
using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Features.Recipes;
using CulinaryBlog.Domain.Entities;
using CulinaryBlog.Domain.Enums;

namespace CulinaryBlog.UnitTests.Recipes;

public class RecipeAuthorizationTests
{
    private static readonly Guid Author = Guid.NewGuid();
    private static readonly Guid Stranger = Guid.NewGuid();
    private static readonly Guid Admin = Guid.NewGuid();
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
        var recipe = new Recipe { AuthorId = Author.ToString() };

        var ex = Assert.Throws<UnauthorizedException>(() => _handler.EnsureCanModify(recipe));
        Assert.Equal("UNAUTHORIZED", ex.Code);
    }

    [Fact]
    public void EnsureCanModify_WhenUserIsNotAuthorAndNotAdmin_ThrowsForbiddenExceptionWithRecipeForbiddenCode()
    {
        _currentUser.UserId = Stranger;
        _currentUser.Role = "User";
        var recipe = new Recipe { AuthorId = Author.ToString() };

        var ex = Assert.Throws<ForbiddenException>(() => _handler.EnsureCanModify(recipe));
        Assert.Equal(RecipeErrorCodes.RecipeForbidden, ex.Code);
    }

    [Fact]
    public void EnsureCanModify_WhenUserIsAuthor_DoesNotThrow()
    {
        _currentUser.UserId = Author;
        var recipe = new Recipe { AuthorId = Author.ToString() };

        var exception = Record.Exception(() => _handler.EnsureCanModify(recipe));
        Assert.Null(exception);
    }

    [Fact]
    public void EnsureCanModify_WhenUserIsAdmin_DoesNotThrowEvenIfNotAuthor()
    {
        _currentUser.UserId = Admin;
        _currentUser.Role = "Admin";
        var recipe = new Recipe { AuthorId = Author.ToString() };

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
            AuthorId = Author.ToString(),
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
        _currentUser.UserId = Stranger;
        var recipe = new Recipe
        {
            Id = Guid.NewGuid(),
            AuthorId = Author.ToString(),
            Status = RecipeStatus.Draft,
            IsDeleted = false
        };

        var ex = Assert.Throws<NotFoundException>(() => _handler.EnsureCanView(recipe));
        Assert.Equal(RecipeErrorCodes.RecipeNotFound, ex.Code);
    }

    [Fact]
    public void EnsureCanView_WhenRecipeIsDraftAndUserIsAuthor_AllowsViewing()
    {
        _currentUser.UserId = Author;
        var recipe = new Recipe
        {
            Id = Guid.NewGuid(),
            AuthorId = Author.ToString(),
            Status = RecipeStatus.Draft,
            IsDeleted = false
        };

        var exception = Record.Exception(() => _handler.EnsureCanView(recipe));
        Assert.Null(exception);
    }

    [Fact]
    public void EnsureCanView_WhenRecipeIsDraftAndUserIsAdmin_AllowsViewing()
    {
        _currentUser.UserId = Admin;
        _currentUser.Role = "Admin";
        var recipe = new Recipe
        {
            Id = Guid.NewGuid(),
            AuthorId = Author.ToString(),
            Status = RecipeStatus.Draft,
            IsDeleted = false
        };

        var exception = Record.Exception(() => _handler.EnsureCanView(recipe));
        Assert.Null(exception);
    }

    private sealed class TestCurrentUser : ICurrentUser
    {
        public Guid? UserId { get; set; }
        public string? Role { get; set; }
        public string? IpAddress => null;
        public bool IsInRole(string role) => string.Equals(Role, role, StringComparison.OrdinalIgnoreCase);
    }
}
