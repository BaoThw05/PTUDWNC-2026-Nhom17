using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Features.Auth;
using CulinaryBlog.Application.Features.Auth.Login;
using CulinaryBlog.Application.Features.Auth.LoginHistory;
using CulinaryBlog.Domain.Auth;
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
        _handler = new LoginCommandHandler(_context.Users, _context.Sessions, _context.Audit);
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

    [Fact]
    public async Task Handle_Success_RecordsLoginWithIpAndDevice()
    {
        var user = _context.Users.Add(Email, Password);

        await _handler.Handle(new LoginCommand(Email, Password), CancellationToken.None);

        var attempt = Assert.Single(_context.LoginAttempts.Attempts);
        Assert.Equal(user.Id, attempt.UserId);
        Assert.Equal(LoginResult.Succeeded, attempt.Result);
        Assert.Equal(LoginMethod.Password, attempt.Method);
        Assert.Equal(_context.CurrentUser.IpAddress, attempt.IpAddress);
        Assert.Equal(_context.CurrentUser.UserAgent, attempt.UserAgent);
        Assert.Equal(_context.Time.GetUtcNow(), attempt.CreatedAt);
    }

    [Fact]
    public async Task Handle_WrongPasswordOrLocked_RecordsFailure()
    {
        var user = _context.Users.Add(Email, Password);

        await Assert.ThrowsAsync<UnauthorizedException>(
            () => _handler.Handle(new LoginCommand(Email, "Wrong@12345"), CancellationToken.None));
        _context.Users.LockedOut.Add(user.Id);
        await Assert.ThrowsAsync<AccountLockedException>(
            () => _handler.Handle(new LoginCommand(Email, Password), CancellationToken.None));

        Assert.Equal(
            [LoginResult.InvalidPassword, LoginResult.LockedOut],
            _context.LoginAttempts.Attempts.Select(attempt => attempt.Result));
    }

    [Fact]
    public async Task Handle_UnknownEmail_RecordsNothing()
    {
        await Assert.ThrowsAsync<UnauthorizedException>(
            () => _handler.Handle(new LoginCommand("nobody@example.com", Password), CancellationToken.None));

        Assert.Empty(_context.LoginAttempts.Attempts);
    }

    [Fact]
    public async Task LoginHistory_ReturnsOnlyCurrentUserNewestFirst()
    {
        var user = _context.Users.Add(Email, Password);
        _context.Users.Add("other@example.com", Password, userName: "other");
        await _handler.Handle(new LoginCommand(Email, Password), CancellationToken.None);
        _context.Time.Advance(TimeSpan.FromMinutes(5));
        await _handler.Handle(new LoginCommand("other@example.com", Password), CancellationToken.None);
        await Assert.ThrowsAsync<UnauthorizedException>(
            () => _handler.Handle(new LoginCommand(Email, "Wrong@12345"), CancellationToken.None));
        _context.CurrentUser.UserId = user.Id;

        var history = await new GetLoginHistoryQueryHandler(_context.CurrentUser, _context.LoginAttempts)
            .Handle(new GetLoginHistoryQuery(), CancellationToken.None);

        Assert.Equal(["InvalidPassword", "Succeeded"], history.Select(entry => entry.Result));
    }
}
