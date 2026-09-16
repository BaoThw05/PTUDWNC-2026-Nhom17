using CulinaryBlog.Application.Features.Auth.Logout;
using CulinaryBlog.Domain.Auth;
using CulinaryBlog.UnitTests.Features.Auth.Fakes;

namespace CulinaryBlog.UnitTests.Features.Auth;

public sealed class LogoutCommandHandlerTests
{
    private readonly AuthTestContext _context = new();
    private readonly LogoutCommandHandler _handler;

    public LogoutCommandHandlerTests()
    {
        _handler = new LogoutCommandHandler(_context.RefreshTokens, _context.CurrentUser, _context.Time);
    }

    [Fact]
    public async Task Handle_OwnToken_RevokesWithLogoutReason()
    {
        var session = await StartSessionAsync();
        _context.CurrentUser.UserId = session.User.Id;

        await _handler.Handle(new LogoutCommand(session.RefreshToken), CancellationToken.None);

        Assert.Equal(RefreshTokenRevokeReason.Logout, _context.RefreshTokens.Tokens.Single().RevokedReason);
    }

    [Fact]
    public async Task Handle_AnonymousCaller_RevokesToken()
    {
        var session = await StartSessionAsync();

        await _handler.Handle(new LogoutCommand(session.RefreshToken), CancellationToken.None);

        Assert.True(_context.RefreshTokens.Tokens.Single().IsRevoked);
    }

    [Fact]
    public async Task Handle_TokenOfAnotherUser_DoesNothing()
    {
        var session = await StartSessionAsync();
        _context.CurrentUser.UserId = Guid.NewGuid();

        await _handler.Handle(new LogoutCommand(session.RefreshToken), CancellationToken.None);

        Assert.False(_context.RefreshTokens.Tokens.Single().IsRevoked);
    }

    [Fact]
    public async Task Handle_UnknownToken_CompletesSilently()
    {
        var exception = await Record.ExceptionAsync(
            () => _handler.Handle(new LogoutCommand("unknown"), CancellationToken.None));

        Assert.Null(exception);
        Assert.Equal(0, _context.RefreshTokens.SaveCount);
    }

    private Task<Application.Features.Auth.Common.AuthResponse> StartSessionAsync()
    {
        var user = _context.Users.Add("author@example.com", "Author@12345");
        return _context.Sessions.StartAsync(user, CancellationToken.None);
    }
}
