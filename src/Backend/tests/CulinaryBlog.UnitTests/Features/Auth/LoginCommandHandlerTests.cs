using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Features.Auth;
using CulinaryBlog.Application.Features.Auth.Login;
using CulinaryBlog.UnitTests.Features.Auth.Fakes;

namespace CulinaryBlog.UnitTests.Features.Auth;

public sealed class LoginCommandHandlerTests
{
    private const string Email = "author@example.com";
    private const string Password = "Author@12345";

    private readonly AuthTestContext _context = new();
    private readonly LoginCommandHandler _handler;

    public LoginCommandHandlerTests()
    {
        _handler = new LoginCommandHandler(_context.Users, _context.Sessions);
    }

    [Fact]
    public async Task Handle_ValidCredentials_ReturnsTokensAndStoresRefreshToken()
    {
        var user = _context.Users.Add(Email, Password);

        var response = await _handler.Handle(new LoginCommand(Email, Password), CancellationToken.None);

        Assert.Equal(user.Id, response.User.Id);
        Assert.NotEmpty(response.RefreshToken);
        Assert.Single(_context.RefreshTokens.Tokens);
    }

    [Theory]
    [InlineData("nobody@example.com", Password)]
    [InlineData(Email, "Wrong@12345")]
    public async Task Handle_UnknownEmailOrWrongPassword_ThrowsSameInvalidCredentials(string email, string password)
    {
        _context.Users.Add(Email, Password);

        var exception = await Assert.ThrowsAsync<UnauthorizedException>(
            () => _handler.Handle(new LoginCommand(email, password), CancellationToken.None));

        Assert.Equal(AuthErrorCodes.InvalidCredentials, exception.Code);
    }

    [Fact]
    public async Task Handle_LockedOutAccount_ThrowsAccountLocked()
    {
        var user = _context.Users.Add(Email, Password);
        _context.Users.LockedOut.Add(user.Id);

        await Assert.ThrowsAsync<AccountLockedException>(
            () => _handler.Handle(new LoginCommand(Email, Password), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_DisabledAccountWithCorrectPassword_ThrowsDisabled()
    {
        _context.Users.Add(Email, Password, isActive: false);

        var exception = await Assert.ThrowsAsync<ForbiddenException>(
            () => _handler.Handle(new LoginCommand(Email, Password), CancellationToken.None));

        Assert.Equal(AuthErrorCodes.AccountDisabled, exception.Code);
    }

    [Fact]
    public async Task Handle_DisabledAccountWithWrongPassword_DoesNotRevealDisabledState()
    {
        _context.Users.Add(Email, Password, isActive: false);

        var exception = await Assert.ThrowsAsync<UnauthorizedException>(
            () => _handler.Handle(new LoginCommand(Email, "Wrong@12345"), CancellationToken.None));

        Assert.Equal(AuthErrorCodes.InvalidCredentials, exception.Code);
    }
}
