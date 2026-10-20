using System.Net;
using System.Net.Http.Json;
using CulinaryBlog.Application.Common.Models;
using CulinaryBlog.Application.Features.Auth;
using CulinaryBlog.Application.Features.Auth.AdminUsers;
using CulinaryBlog.Domain.Auth;
using CulinaryBlog.IntegrationTests.Infrastructure;
using static CulinaryBlog.IntegrationTests.Auth.AuthApi;

namespace CulinaryBlog.IntegrationTests.Auth;

[Collection(PostgresCollection.Name)]
public sealed class AdminUsersEndpointsTests(PostgresApiFactory factory)
{
    private const string UsersPath = "/api/v1/admin/users";

    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task List_AsAdmin_FindsUserBySearch()
    {
        var email = NewEmail();
        await RegisterAsync(_client, email);
        var admin = await LoginAdminAsync();

        var response = await _client.SendAsync(
            WithBearer(HttpMethod.Get, $"{UsersPath}?search={email}", admin.AccessToken));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var page = await response.Content.ReadFromJsonAsync<PagedResult<AdminUserResponse>>();
        var user = Assert.Single(page!.Items);
        Assert.Equal(email, user.Email);
        Assert.True(user.IsActive);
        Assert.Equal([Roles.Author], user.Roles);
    }

    [Fact]
    public async Task List_AsAuthor_Returns403()
    {
        var author = await ReadAuthAsync(await RegisterAsync(_client, NewEmail()));

        var response = await _client.SendAsync(WithBearer(HttpMethod.Get, UsersPath, author.AccessToken));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Disable_BlocksLoginAndRefresh_EnableRestoresLogin()
    {
        var email = NewEmail();
        var author = await ReadAuthAsync(await RegisterAsync(_client, email));
        var admin = await LoginAdminAsync();

        var disable = await PatchAsync(admin.AccessToken, author.User.Id, new { isActive = false });
        Assert.Equal(HttpStatusCode.OK, disable.StatusCode);

        var login = await LoginAsync(_client, email, StrongPassword);
        Assert.Equal(HttpStatusCode.Forbidden, login.StatusCode);
        Assert.Equal(AuthErrorCodes.AccountDisabled, await ReadErrorCodeAsync(login));
        var refresh = await RefreshAsync(_client, author.RefreshToken);
        Assert.Equal(HttpStatusCode.Unauthorized, refresh.StatusCode);

        await PatchAsync(admin.AccessToken, author.User.Id, new { isActive = true });
        Assert.Equal(HttpStatusCode.OK, (await LoginAsync(_client, email, StrongPassword)).StatusCode);
    }

    [Fact]
    public async Task Update_AdminDemotesSelf_Returns403()
    {
        var admin = await LoginAdminAsync();

        var response = await PatchAsync(admin.AccessToken, admin.User.Id, new { roles = new[] { Roles.Author } });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.Equal(AuthErrorCodes.AdminSelfLockout, await ReadErrorCodeAsync(response));
    }

    [Fact]
    public async Task Update_UnknownRole_Returns422()
    {
        var author = await ReadAuthAsync(await RegisterAsync(_client, NewEmail()));
        var admin = await LoginAdminAsync();

        var response = await PatchAsync(admin.AccessToken, author.User.Id, new { roles = new[] { "Root" } });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    private async Task<Application.Features.Auth.Common.AuthResponse> LoginAdminAsync() =>
        await ReadAuthAsync(await LoginAsync(_client, PostgresApiFactory.AdminEmail, PostgresApiFactory.AdminPassword));

    private Task<HttpResponseMessage> PatchAsync(string accessToken, Guid userId, object body) =>
        _client.SendAsync(WithBearer(HttpMethod.Patch, $"{UsersPath}/{userId}", accessToken, body));
}
