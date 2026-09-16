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
            new RegisterCommand(" new@example.com ", "Author@12345", "  Tuấn "),
            CancellationToken.None);

        Assert.Equal("new@example.com", response.User.Email);
        Assert.Equal("Tuấn", response.User.DisplayName);
        Assert.Contains(Roles.Author, response.User.Roles);
        Assert.Single(_context.RefreshTokens.Tokens);
    }

    [Fact]
    public async Task Handle_ExistingEmail_ThrowsEmailExists()
    {
        _context.Users.Add("taken@example.com", "Author@12345");

        var exception = await Assert.ThrowsAsync<ConflictException>(() => _handler.Handle(
            new RegisterCommand("TAKEN@example.com", "Author@12345", "Someone"),
            CancellationToken.None));

        Assert.Equal(AuthErrorCodes.EmailExists, exception.Code);
    }
}
