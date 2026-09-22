using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using CulinaryBlog.Application.Features.Auth.Common;

namespace CulinaryBlog.IntegrationTests.Auth;

/// <summary>Gọi các endpoint /api/v1/auth trong test cho gọn.</summary>
internal static class AuthApi
{
    public const string BasePath = "/api/v1/auth";
    public const string StrongPassword = "Author@12345";

    public static Task<HttpResponseMessage> RegisterAsync(HttpClient client, string email, string password = StrongPassword) =>
        client.PostAsJsonAsync(
            $"{BasePath}/register",
            new { fullName = "Người Thử", email, userName = NewUserName(), password });

    public static Task<HttpResponseMessage> LoginAsync(HttpClient client, string email, string password) =>
        client.PostAsJsonAsync($"{BasePath}/login", new { email, password });

    public static Task<HttpResponseMessage> RefreshAsync(HttpClient client, string refreshToken) =>
        client.PostAsJsonAsync($"{BasePath}/refresh", new { refreshToken });

    public static Task<HttpResponseMessage> LogoutAsync(HttpClient client, string refreshToken) =>
        client.PostAsJsonAsync($"{BasePath}/logout", new { refreshToken });

    public static async Task<AuthResponse> ReadAuthAsync(HttpResponseMessage response)
    {
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<AuthResponse>())!;
    }

    public static async Task<string?> ReadErrorCodeAsync(HttpResponseMessage response)
    {
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return body.RootElement.TryGetProperty("code", out var code) ? code.GetString() : null;
    }

    public static HttpRequestMessage WithBearer(HttpMethod method, string path, string accessToken, object? body = null)
    {
        var request = new HttpRequestMessage(method, path);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        if (body is not null)
        {
            request.Content = JsonContent.Create(body);
        }

        return request;
    }

    public static string NewEmail() => $"user-{Guid.NewGuid():N}@example.com";

    public static string NewUserName() => $"user{Guid.NewGuid():N}";
}
