using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Features.Auth;
using CulinaryBlog.Application.Features.Auth.Register;
using CulinaryBlog.Domain.Auth;
using CulinaryBlog.UnitTests.Features.Auth.Fakes;

namespace CulinaryBlog.UnitTests.Features.Auth;

public sealed class RegisterCommandHandlerTests
{
    private readonly AuthTestContext _context = new();
    private readonly RegisterCommandHandler _handler;

    public RegisterCommandHandlerTests()
    {
        _handler = new RegisterCommandHandler(_context.Users, _context.Sessions);
    }

    [Fact]
    public async Task Handle_NewEmail_CreatesAuthorAndSignsIn()
    {
        var response = await _handler.Handle(
            new RegisterCommand("  Tuấn ", " new@example.com ", " tuan99 ", "Author@12345"),
            CancellationToken.None);

        Assert.Equal("new@example.com", response.User.Email);
        Assert.Equal("tuan99", response.User.UserName);
        Assert.Equal("Tuấn", response.User.FullName);
        Assert.Contains(Roles.Author, response.User.Roles);
        Assert.Single(_context.RefreshTokens.Tokens);
    }

    [Fact]
    public async Task Handle_ExistingEmail_ThrowsEmailExists()
    {
        _context.Users.Add("taken@example.com", "Author@12345");

        var exception = await Assert.ThrowsAsync<ConflictException>(() => _handler.Handle(
            new RegisterCommand("Someone", "TAKEN@example.com", "someoneelse", "Author@12345"),
            CancellationToken.None));

        Assert.Equal(AuthErrorCodes.EmailExists, exception.Code);
    }

    [Fact]
    public async Task Handle_ExistingUserName_ThrowsUserNameExists()
    {
        _context.Users.Add("taken@example.com", "Author@12345", userName: "tuan99");

        var exception = await Assert.ThrowsAsync<ConflictException>(() => _handler.Handle(
            new RegisterCommand("Someone", "new@example.com", "TUAN99", "Author@12345"),
            CancellationToken.None));

        Assert.Equal(AuthErrorCodes.UserNameExists, exception.Code);
    }
}
