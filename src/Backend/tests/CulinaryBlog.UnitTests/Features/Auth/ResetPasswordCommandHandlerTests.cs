using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Features.Auth;
using CulinaryBlog.Application.Features.Auth.Abstractions;
using CulinaryBlog.Application.Features.Auth.ResetPassword;
using CulinaryBlog.Domain.Auth;
using CulinaryBlog.UnitTests.Features.Auth.Fakes;

namespace CulinaryBlog.UnitTests.Features.Auth;

public sealed class ResetPasswordCommandHandlerTests
{
    private const string Password = "Author@12345";
    private const string NewPassword = "Changed@67890";

    private readonly AuthTestContext _context = new();
    private readonly ResetPasswordCommandHandler _handler;

    public ResetPasswordCommandHandlerTests()
    {
        _handler = new ResetPasswordCommandHandler(_context.Users, _context.RefreshTokens, _context.Time);
    }

    [Fact]
    public async Task Handle_ValidToken_ChangesPasswordAndRevokesEverySession()
    {
        var user = _context.Users.Add("author@example.com", Password);
        await _context.Sessions.StartAsync(user, CancellationToken.None);

        await _handler.Handle(new ResetPasswordCommand(user.Id, $"reset-{user.Id}", NewPassword), CancellationToken.None);

        Assert.Equal(
            PasswordCheckResult.Success,
            await _context.Users.CheckPasswordAsync(user.Id, NewPassword, CancellationToken.None));
        Assert.All(
            _context.RefreshTokens.Tokens,
            token => Assert.Equal(RefreshTokenRevokeReason.PasswordChanged, token.RevokedReason));
    }

    [Fact]
    public async Task Handle_WrongToken_ThrowsResetTokenInvalidAndKeepsSessions()
    {
        var user = _context.Users.Add("author@example.com", Password);
        await _context.Sessions.StartAsync(user, CancellationToken.None);

        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => _handler.Handle(new ResetPasswordCommand(user.Id, "bad-token", NewPassword), CancellationToken.None));

        Assert.Equal(AuthErrorCodes.ResetTokenInvalid, exception.Code);
        Assert.False(_context.RefreshTokens.Tokens.Single().IsRevoked);
    }

    [Fact]
    public async Task Handle_UnknownUser_ThrowsResetTokenInvalid()
    {
        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => _handler.Handle(new ResetPasswordCommand(Guid.NewGuid(), "any", NewPassword), CancellationToken.None));

        Assert.Equal(AuthErrorCodes.ResetTokenInvalid, exception.Code);
    }
}
