using System.Net;
using System.Net.Http.Json;
using CulinaryBlog.Application.Features.Auth;
using CulinaryBlog.Application.Features.Auth.Common;
using CulinaryBlog.Application.Features.Auth.LoginHistory;
using CulinaryBlog.Infrastructure.Auth;
using CulinaryBlog.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using static CulinaryBlog.IntegrationTests.Auth.AuthApi;

namespace CulinaryBlog.IntegrationTests.Auth;

/// <summary>Nhật ký đăng nhập, xác nhận email và đổi email bằng OTP (hướng phát triển sau 25/10).</summary>
[Collection(PostgresCollection.Name)]
public sealed class AccountSecurityEndpointsTests(PostgresApiFactory factory)
{
    private const string Device = "Mozilla/5.0 (IntegrationTest)";

    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task LoginHistory_RecordsSuccessAndFailureWithDevice()
    {
        var email = NewEmail();
        await RegisterAsync(_client, email);
        _client.DefaultRequestHeaders.UserAgent.ParseAdd(Device);
        await LoginAsync(_client, email, "Wrong@12345");
        var auth = await ReadAuthAsync(await LoginAsync(_client, email, StrongPassword));

        var response = await _client.SendAsync(WithBearer(HttpMethod.Get, $"{BasePath}/me/logins", auth.AccessToken));

        response.EnsureSuccessStatusCode();
        var history = (await response.Content.ReadFromJsonAsync<LoginAttemptResponse[]>())!;
        Assert.Equal(["Succeeded", "InvalidPassword"], history.Select(entry => entry.Result));
        Assert.All(history, entry => Assert.Equal(Device, entry.UserAgent));
        Assert.All(history, entry => Assert.Equal("Password", entry.Method));
    }

    [Fact]
    public async Task LoginHistory_WithoutToken_Returns401()
    {
        var response = await _client.GetAsync($"{BasePath}/me/logins");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ConfirmEmail_ValidToken_MarksProfileConfirmed()
    {
        var auth = await ReadAuthAsync(await RegisterAsync(_client, NewEmail()));
        Assert.False(auth.User.EmailConfirmed);

        var response = await _client.PostAsJsonAsync(
            $"{BasePath}/confirm-email",
            new { userId = auth.User.Id, token = await CreateConfirmationTokenAsync(auth.User.Email) });

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.True((await GetProfileAsync(auth.AccessToken)).EmailConfirmed);
    }

    [Fact]
    public async Task ConfirmEmail_InvalidToken_Returns422()
    {
        var auth = await ReadAuthAsync(await RegisterAsync(_client, NewEmail()));

        var response = await _client.PostAsJsonAsync(
            $"{BasePath}/confirm-email",
            new { userId = auth.User.Id, token = "invalid-token" });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        Assert.Equal(AuthErrorCodes.EmailTokenInvalid, await ReadErrorCodeAsync(response));
    }

    [Fact]
    public async Task ChangeEmail_WithCodeFromIdentity_UpdatesEmailAndLoginUsesNewEmail()
    {
        var auth = await ReadAuthAsync(await RegisterAsync(_client, NewEmail()));
        var newEmail = NewEmail();

        var request = await _client.SendAsync(WithBearer(
            HttpMethod.Post,
            $"{BasePath}/me/email-change",
            auth.AccessToken,
            new { newEmail, currentPassword = StrongPassword }));
        var confirm = await _client.SendAsync(WithBearer(
            HttpMethod.Post,
            $"{BasePath}/me/email-change/confirm",
            auth.AccessToken,
            new { newEmail, code = await CreateChangeEmailCodeAsync(auth.User.Email, newEmail) }));

        Assert.Equal(HttpStatusCode.NoContent, request.StatusCode);
        confirm.EnsureSuccessStatusCode();
        var profile = (await confirm.Content.ReadFromJsonAsync<UserProfileResponse>())!;
        Assert.Equal(newEmail, profile.Email);
        Assert.True(profile.EmailConfirmed);
        Assert.Equal(HttpStatusCode.OK, (await LoginAsync(_client, newEmail, StrongPassword)).StatusCode);
    }

    [Fact]
    public async Task ChangeEmail_WrongCode_Returns422()
    {
        var auth = await ReadAuthAsync(await RegisterAsync(_client, NewEmail()));

        var response = await _client.SendAsync(WithBearer(
            HttpMethod.Post,
            $"{BasePath}/me/email-change/confirm",
            auth.AccessToken,
            new { newEmail = NewEmail(), code = "000000" }));

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        Assert.Equal(AuthErrorCodes.EmailCodeInvalid, await ReadErrorCodeAsync(response));
    }

    [Fact]
    public async Task ChangeEmail_WrongPasswordOrTakenEmail_Rejected()
    {
        var auth = await ReadAuthAsync(await RegisterAsync(_client, NewEmail()));
        var takenEmail = NewEmail();
        await RegisterAsync(_client, takenEmail);

        var wrongPassword = await _client.SendAsync(WithBearer(
            HttpMethod.Post,
            $"{BasePath}/me/email-change",
            auth.AccessToken,
            new { newEmail = NewEmail(), currentPassword = "Wrong@12345" }));
        var taken = await _client.SendAsync(WithBearer(
            HttpMethod.Post,
            $"{BasePath}/me/email-change",
            auth.AccessToken,
            new { newEmail = takenEmail, currentPassword = StrongPassword }));

        Assert.Equal(HttpStatusCode.UnprocessableEntity, wrongPassword.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, taken.StatusCode);
        Assert.Equal(AuthErrorCodes.EmailExists, await ReadErrorCodeAsync(taken));
    }

    private async Task<UserProfileResponse> GetProfileAsync(string accessToken)
    {
        var response = await _client.SendAsync(WithBearer(HttpMethod.Get, $"{BasePath}/me", accessToken));
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<UserProfileResponse>())!;
    }

    private async Task<string> CreateConfirmationTokenAsync(string email)
    {
        using var scope = factory.Services.CreateScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        return await users.GenerateEmailConfirmationTokenAsync((await users.FindByEmailAsync(email))!);
    }

    private async Task<string> CreateChangeEmailCodeAsync(string email, string newEmail)
    {
        using var scope = factory.Services.CreateScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        return await users.GenerateChangeEmailTokenAsync((await users.FindByEmailAsync(email))!, newEmail);
    }
}
