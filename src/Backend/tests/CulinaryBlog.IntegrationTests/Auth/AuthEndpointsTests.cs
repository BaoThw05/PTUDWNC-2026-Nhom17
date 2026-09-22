using System.Net;
using System.Net.Http.Json;
using CulinaryBlog.Application.Features.Auth;
using CulinaryBlog.Application.Features.Auth.Common;
using CulinaryBlog.Domain.Auth;
using CulinaryBlog.Infrastructure.Auth;
using CulinaryBlog.Infrastructure.Persistence;
using CulinaryBlog.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using static CulinaryBlog.IntegrationTests.Auth.AuthApi;

namespace CulinaryBlog.IntegrationTests.Auth;

[Collection(PostgresCollection.Name)]
public sealed class AuthEndpointsTests(PostgresApiFactory factory)
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Register_NewEmail_Returns201WithAuthorSession()
    {
        var email = NewEmail();

        var response = await RegisterAsync(_client, email);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal("/api/v1/auth/me", response.Headers.Location?.OriginalString);
        var auth = await ReadAuthAsync(response);
        Assert.Equal(email, auth.User.Email);
        Assert.Equal("Người Thử", auth.User.FullName);
        Assert.Equal([Roles.Author], auth.User.Roles);
        Assert.NotEmpty(auth.AccessToken);
        Assert.NotEmpty(auth.RefreshToken);
    }

    [Fact]
    public async Task Register_DuplicateEmail_Returns409()
    {
        var email = NewEmail();
        await RegisterAsync(_client, email);

        var response = await RegisterAsync(_client, email.ToUpperInvariant());

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal(AuthErrorCodes.EmailExists, await ReadErrorCodeAsync(response));
    }

    [Fact]
    public async Task Register_DuplicateUserName_Returns409()
    {
        var userName = NewUserName();
        await _client.PostAsJsonAsync(
            $"{BasePath}/register",
            new { fullName = "Người Thử", email = NewEmail(), userName, password = StrongPassword });

        var response = await _client.PostAsJsonAsync(
            $"{BasePath}/register",
            new { fullName = "Người Thử", email = NewEmail(), userName = userName.ToUpperInvariant(), password = StrongPassword });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal(AuthErrorCodes.UserNameExists, await ReadErrorCodeAsync(response));
    }

    [Fact]
    public async Task Register_WeakPassword_Returns422()
    {
        var response = await RegisterAsync(_client, NewEmail(), "weak");

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        Assert.Equal("VALIDATION_ERROR", await ReadErrorCodeAsync(response));
    }

    [Fact]
    public async Task Login_SeededAdmin_ReturnsAdminAndAuthorRoles()
    {
        var response = await LoginAsync(_client, PostgresApiFactory.AdminEmail, PostgresApiFactory.AdminPassword);

        var auth = await ReadAuthAsync(response);
        Assert.Contains(Roles.Admin, auth.User.Roles);
        Assert.Contains(Roles.Author, auth.User.Roles);
    }

    [Fact]
    public async Task Login_WrongPassword_Returns401()
    {
        var response = await LoginAsync(_client, PostgresApiFactory.AuthorEmail, "Wrong@12345");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal(AuthErrorCodes.InvalidCredentials, await ReadErrorCodeAsync(response));
    }

    [Fact]
    public async Task Login_FiveWrongPasswords_LocksAccountEvenForCorrectPassword()
    {
        var email = NewEmail();
        await RegisterAsync(_client, email);

        for (var attempt = 0; attempt < 5; attempt++)
        {
            await LoginAsync(_client, email, "Wrong@12345");
        }

        var response = await LoginAsync(_client, email, StrongPassword);

        Assert.Equal(HttpStatusCode.Locked, response.StatusCode);
        Assert.Equal(AuthErrorCodes.AccountLocked, await ReadErrorCodeAsync(response));
    }

    [Fact]
    public async Task Login_DisabledAccount_Returns403()
    {
        var email = NewEmail();
        await RegisterAsync(_client, email);
        await SetActiveAsync(email, isActive: false);

        var response = await LoginAsync(_client, email, StrongPassword);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.Equal(AuthErrorCodes.AccountDisabled, await ReadErrorCodeAsync(response));
    }

    [Fact]
    public async Task Me_WithAccessToken_ReturnsProfile()
    {
        var auth = await RegisterNewAsync();

        var response = await _client.SendAsync(WithBearer(HttpMethod.Get, $"{BasePath}/me", auth.AccessToken));

        var profile = await response.Content.ReadFromJsonAsync<UserProfileResponse>();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(auth.User.Id, profile!.Id);
    }

    [Fact]
    public async Task Me_WithoutAccessToken_Returns401()
    {
        var response = await _client.GetAsync($"{BasePath}/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal("UNAUTHORIZED", await ReadErrorCodeAsync(response));
    }

    [Fact]
    public async Task UpdateProfile_ValidName_ReturnsUpdatedProfile()
    {
        var auth = await RegisterNewAsync();

        var response = await _client.SendAsync(WithBearer(
            HttpMethod.Patch,
            $"{BasePath}/me",
            auth.AccessToken,
            new { fullName = "  Tên Mới  " }));

        var profile = await response.Content.ReadFromJsonAsync<UserProfileResponse>();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Tên Mới", profile!.FullName);
    }

    [Fact]
    public async Task UpdateProfile_TooShortName_Returns422()
    {
        var auth = await RegisterNewAsync();

        var response = await _client.SendAsync(WithBearer(
            HttpMethod.Patch,
            $"{BasePath}/me",
            auth.AccessToken,
            new { fullName = "A" }));

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    [Fact]
    public async Task Refresh_ValidToken_RotatesAndAllowsReuseWithinGracePeriod()
    {
        var auth = await RegisterNewAsync();

        var rotated = await ReadAuthAsync(await RefreshAsync(_client, auth.RefreshToken));
        var retried = await RefreshAsync(_client, auth.RefreshToken);

        Assert.NotEqual(auth.RefreshToken, rotated.RefreshToken);
        Assert.Equal(HttpStatusCode.OK, retried.StatusCode);
    }

    [Fact]
    public async Task Refresh_ReusedAfterGracePeriod_RevokesWholeFamily()
    {
        var auth = await RegisterNewAsync();
        var rotated = await ReadAuthAsync(await RefreshAsync(_client, auth.RefreshToken));
        await UpdateTokenAsync(auth.RefreshToken, setters => setters
            .SetProperty(token => token.RevokedAt, DateTimeOffset.UtcNow.AddMinutes(-5)));

        var reuse = await RefreshAsync(_client, auth.RefreshToken);
        var latest = await RefreshAsync(_client, rotated.RefreshToken);

        Assert.Equal(HttpStatusCode.Unauthorized, reuse.StatusCode);
        Assert.Equal(AuthErrorCodes.RefreshTokenRevoked, await ReadErrorCodeAsync(reuse));
        Assert.Equal(HttpStatusCode.Unauthorized, latest.StatusCode);
    }

    [Fact]
    public async Task Refresh_ExpiredToken_Returns401Expired()
    {
        var auth = await RegisterNewAsync();
        await UpdateTokenAsync(auth.RefreshToken, setters => setters
            .SetProperty(token => token.ExpiresAt, DateTimeOffset.UtcNow.AddMinutes(-1)));

        var response = await RefreshAsync(_client, auth.RefreshToken);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal(AuthErrorCodes.RefreshTokenExpired, await ReadErrorCodeAsync(response));
    }

    [Fact]
    public async Task Logout_ThenRefresh_Returns401WithoutRevokingFamily()
    {
        var auth = await RegisterNewAsync();
        var rotated = await ReadAuthAsync(await RefreshAsync(_client, auth.RefreshToken));

        var logout = await LogoutAsync(_client, rotated.RefreshToken);
        var refresh = await RefreshAsync(_client, rotated.RefreshToken);

        Assert.Equal(HttpStatusCode.NoContent, logout.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, refresh.StatusCode);
        Assert.Equal(AuthErrorCodes.RefreshTokenRevoked, await ReadErrorCodeAsync(refresh));
        Assert.False(await AnyTokenRevokedForReuseAsync(auth.User.Id));
    }

    [Fact]
    public async Task Logout_UnknownToken_Returns204()
    {
        var response = await LogoutAsync(_client, "not-a-real-token");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Google_NotConfigured_Returns502()
    {
        var response = await _client.PostAsJsonAsync($"{BasePath}/google", new { idToken = "fake" });

        Assert.Equal(HttpStatusCode.BadGateway, response.StatusCode);
        Assert.Equal(AuthErrorCodes.GoogleUnavailable, await ReadErrorCodeAsync(response));
    }

    private async Task<AuthResponse> RegisterNewAsync() => await ReadAuthAsync(await RegisterAsync(_client, NewEmail()));

    private async Task SetActiveAsync(string email, bool isActive)
    {
        using var scope = factory.Services.CreateScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = await users.FindByEmailAsync(email);
        user!.IsActive = isActive;
        await users.UpdateAsync(user);
    }

    private async Task UpdateTokenAsync(
        string plainToken,
        Action<Microsoft.EntityFrameworkCore.Query.UpdateSettersBuilder<RefreshToken>> setters)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var hash = RefreshTokenValue.Hash(plainToken);
        await db.RefreshTokens.Where(token => token.TokenHash == hash).ExecuteUpdateAsync(setters);
    }

    private async Task<bool> AnyTokenRevokedForReuseAsync(Guid userId)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        return await db.RefreshTokens.AnyAsync(token =>
            token.UserId == userId && token.RevokedReason == RefreshTokenRevokeReason.ReuseDetected);
    }
}
