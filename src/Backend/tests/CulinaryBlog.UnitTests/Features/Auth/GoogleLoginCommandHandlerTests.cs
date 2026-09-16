using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Features.Auth;
using CulinaryBlog.Application.Features.Auth.Abstractions;
using CulinaryBlog.Application.Features.Auth.GoogleLogin;
using CulinaryBlog.UnitTests.Features.Auth.Fakes;

namespace CulinaryBlog.UnitTests.Features.Auth;

public sealed class GoogleLoginCommandHandlerTests
{
    private const string Picture = "https://example.com/avatar.png";

    private readonly AuthTestContext _context = new();

    [Fact]
    public async Task Handle_NewGoogleUser_CreatesAccountWithAvatarAndLinksLogin()
    {
        var identity = new GoogleIdentity("google-sub", "new@example.com", EmailVerified: true, "Ngọc Tuấn", Picture);

        var response = await HandleAsync(identity);

        Assert.Equal("Ngọc Tuấn", response.User.DisplayName);
        Assert.Equal(Picture, response.User.AvatarUrl);
        Assert.True(_context.Users.HasLogin(new ExternalLogin(GoogleLoginCommandHandler.ProviderName, "google-sub")));
    }

    [Fact]
    public async Task Handle_ExistingLocalAccount_LinksInsteadOfCreating()
    {
        var existing = _context.Users.Add("local@example.com", "Author@12345");
        var identity = new GoogleIdentity("google-sub", "local@example.com", EmailVerified: true, null, Picture);

        var response = await HandleAsync(identity);

        Assert.Equal(existing.Id, response.User.Id);
        Assert.Equal(Picture, response.User.AvatarUrl);
    }

    [Fact]
    public async Task Handle_UnverifiedEmail_ThrowsWithoutLinking()
    {
        _context.Users.Add("local@example.com", "Author@12345");
        var identity = new GoogleIdentity("google-sub", "local@example.com", EmailVerified: false, null, null);

        var exception = await Assert.ThrowsAsync<UnauthorizedException>(() => HandleAsync(identity));

        Assert.Equal(AuthErrorCodes.GoogleEmailUnverified, exception.Code);
        Assert.False(_context.Users.HasLogin(new ExternalLogin(GoogleLoginCommandHandler.ProviderName, "google-sub")));
    }

    [Fact]
    public async Task Handle_ShortGoogleName_FallsBackToEmailPrefix()
    {
        var identity = new GoogleIdentity("google-sub", "tuan@example.com", EmailVerified: true, "T", null);

        var response = await HandleAsync(identity);

        Assert.Equal("tuan", response.User.DisplayName);
    }

    private Task<Application.Features.Auth.Common.AuthResponse> HandleAsync(GoogleIdentity identity)
    {
        var handler = new GoogleLoginCommandHandler(new StubGoogleValidator(identity), _context.Users, _context.Sessions);
        return handler.Handle(new GoogleLoginCommand("id-token"), CancellationToken.None);
    }

    private sealed class StubGoogleValidator(GoogleIdentity identity) : IGoogleIdTokenValidator
    {
        public Task<GoogleIdentity> ValidateAsync(string idToken, CancellationToken cancellationToken) =>
            Task.FromResult(identity);
    }
}
