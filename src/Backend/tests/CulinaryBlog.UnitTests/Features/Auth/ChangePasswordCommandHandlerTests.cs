using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Features.Auth;
using CulinaryBlog.Application.Features.Auth.Abstractions;
using CulinaryBlog.Application.Features.Auth.ChangePassword;
using CulinaryBlog.Domain.Auth;
using CulinaryBlog.UnitTests.Features.Auth.Fakes;

namespace CulinaryBlog.UnitTests.Features.Auth;

public sealed class ChangePasswordCommandHandlerTests
{
    private const string Password = "Author@12345";
    private const string NewPassword = "Changed@67890";

    private readonly AuthTestContext _context = new();
    private readonly ChangePasswordCommandHandler _handler;

    public ChangePasswordCommandHandlerTests()
    {
        _handler = new ChangePasswordCommandHandler(
            _context.CurrentUser,
            _context.Users,
            _context.RefreshTokens,
            _context.Time);
    }

    [Fact]
    public async Task Handle_CorrectPassword_ChangesPasswordAndRevokesEverySession()
    {
        var user = _context.Users.Add("author@example.com", Password);
        await _context.Sessions.StartAsync(user, CancellationToken.None);
        await _context.Sessions.StartAsync(user, CancellationToken.None);
        _context.CurrentUser.UserId = user.Id;

        await _handler.Handle(new ChangePasswordCommand(Password, NewPassword), CancellationToken.None);

        Assert.Equal(
            PasswordCheckResult.Success,
            await _context.Users.CheckPasswordAsync(user.Id, NewPassword, CancellationToken.None));
        Assert.All(
            _context.RefreshTokens.Tokens,
            token => Assert.Equal(RefreshTokenRevokeReason.PasswordChanged, token.RevokedReason));
    }

    [Fact]
    public async Task Handle_WrongCurrentPassword_ThrowsValidationOnCurrentPasswordAndKeepsSessions()
    {
        var user = _context.Users.Add("author@example.com", Password);
        await _context.Sessions.StartAsync(user, CancellationToken.None);
        _context.CurrentUser.UserId = user.Id;

        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => _handler.Handle(new ChangePasswordCommand("Wrong@12345", NewPassword), CancellationToken.None));

        Assert.Contains("CurrentPassword", exception.Errors.Keys);
        Assert.False(_context.RefreshTokens.Tokens.Single().IsRevoked);
    }

    [Fact]
    public async Task Handle_LockedOutAccount_ThrowsAccountLocked()
    {
        var user = _context.Users.Add("author@example.com", Password);
        _context.Users.LockedOut.Add(user.Id);
        _context.CurrentUser.UserId = user.Id;

        await Assert.ThrowsAsync<AccountLockedException>(
            () => _handler.Handle(new ChangePasswordCommand(Password, NewPassword), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_Anonymous_ThrowsUnauthorized()
    {
        await Assert.ThrowsAsync<UnauthorizedException>(
            () => _handler.Handle(new ChangePasswordCommand(Password, NewPassword), CancellationToken.None));
    }

    [Theory]
    [InlineData("weak")]
    [InlineData(Password)]
    public void Validator_RejectsWeakOrUnchangedPassword(string newPassword)
    {
        var result = new ChangePasswordCommandValidator().Validate(new ChangePasswordCommand(Password, newPassword));

        Assert.False(result.IsValid);
    }
}
