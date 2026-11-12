using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Features.Auth;
using CulinaryBlog.Application.Features.Auth.AdminUsers;
using CulinaryBlog.Domain.Auth;
using CulinaryBlog.UnitTests.Features.Auth.Fakes;

namespace CulinaryBlog.UnitTests.Features.Auth;

public sealed class UpdateUserCommandHandlerTests
{
    private readonly AuthTestContext _context = new();
    private readonly UpdateUserCommandHandler _handler;

    public UpdateUserCommandHandlerTests()
    {
        _handler = new UpdateUserCommandHandler(
            _context.CurrentUser,
            _context.Users,
            _context.RefreshTokens,
            _context.Time);
        _context.CurrentUser.UserId = Guid.NewGuid();
    }

    [Fact]
    public async Task Handle_Disable_DeactivatesAndRevokesEverySession()
    {
        var user = _context.Users.Add("author@example.com", "Author@12345");
        await _context.Sessions.StartAsync(user, CancellationToken.None);
        await _context.Sessions.StartAsync(user, CancellationToken.None);

        var response = await _handler.Handle(new UpdateUserCommand(user.Id, false, null), CancellationToken.None);

        Assert.False(response.IsActive);
        Assert.All(
            _context.RefreshTokens.Tokens,
            token => Assert.Equal(RefreshTokenRevokeReason.AccountDisabled, token.RevokedReason));
    }

    [Fact]
    public async Task Handle_SetRoles_KeepsSessions()
    {
        var user = _context.Users.Add("author@example.com", "Author@12345");
        await _context.Sessions.StartAsync(user, CancellationToken.None);

        var response = await _handler.Handle(
            new UpdateUserCommand(user.Id, null, [Roles.Admin, Roles.Author]),
            CancellationToken.None);

        Assert.Equal([Roles.Admin, Roles.Author], response.Roles);
        Assert.False(_context.RefreshTokens.Tokens.Single().IsRevoked);
    }

    [Theory]
    [InlineData(false, null)]
    [InlineData(null, Roles.Author)]
    public async Task Handle_AdminLocksOutSelf_ThrowsForbidden(bool? isActive, string? onlyRole)
    {
        var admin = _context.Users.Add("admin@example.com", "Admin@12345");
        _context.CurrentUser.UserId = admin.Id;
        IReadOnlyList<string>? roles = onlyRole is null ? null : [onlyRole];

        var exception = await Assert.ThrowsAsync<ForbiddenException>(
            () => _handler.Handle(new UpdateUserCommand(admin.Id, isActive, roles), CancellationToken.None));

        Assert.Equal(AuthErrorCodes.AdminSelfLockout, exception.Code);
    }

    [Fact]
    public void Validator_UnknownRoleOrEmptyBody_Fails()
    {
        var validator = new UpdateUserCommandValidator();

        Assert.False(validator.Validate(new UpdateUserCommand(Guid.NewGuid(), null, ["Root"])).IsValid);
        Assert.False(validator.Validate(new UpdateUserCommand(Guid.NewGuid(), null, null)).IsValid);
        Assert.True(validator.Validate(new UpdateUserCommand(Guid.NewGuid(), true, null)).IsValid);
    }
}
