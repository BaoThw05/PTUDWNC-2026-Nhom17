using System.Net;
using CulinaryBlog.Application.Abstractions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace CulinaryBlog.IntegrationTests;

/// <summary>
/// Fixture dùng chung cho integration test. Đăng ký ICurrentUser giả
/// để app có thể khởi động được khi TV1 chưa merge Auth_Init vào main.
/// </summary>
public sealed class IntegrationTestFactory : WebApplicationFactory<Program>
{
    public static readonly TestCurrentUser CurrentUser = new();

    protected override void ConfigureWebHost(Microsoft.AspNetCore.Hosting.IWebHostBuilder builder)
    {
        builder.UseSetting("environment", "Development");
        builder.ConfigureServices(services =>
        {
            // Stub ICurrentUser: chờ TV1 merge Auth_Init thì bỏ stub này,
            // lúc đó HttpCurrentUser của TV1 sẽ được đăng ký qua AddAuthApi.
            services.AddScoped<ICurrentUser>(_ => CurrentUser);
        });
    }

    public sealed class TestCurrentUser : ICurrentUser
    {
        public Guid? UserId { get; set; }
        public string? IpAddress => "127.0.0.1";
        public string? Role { get; set; }

        public bool IsInRole(string role) =>
            string.Equals(Role, role, StringComparison.OrdinalIgnoreCase);

        public void SetAsUser(Guid id, string role = "Author")
        {
            UserId = id;
            Role = role;
        }

        public void Reset()
        {
            UserId = null;
            Role = null;
        }
    }
}
