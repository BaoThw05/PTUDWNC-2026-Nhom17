using Testcontainers.PostgreSql;

namespace CulinaryBlog.IntegrationTests.Infrastructure;

/// <summary>API chạy với PostgreSQL thật trong container (cần Docker đang chạy); migrate và seed khi khởi động.</summary>
public sealed class PostgresApiFactory : ApiFactory, IAsyncLifetime
{
    public const string AdminEmail = "admin@culinaryblog.test";
    public const string AdminPassword = "Admin@12345";
    public const string AuthorEmail = "author1@culinaryblog.test";
    public const string AuthorPassword = "Author@12345";

    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:16.15-alpine").Build();

    protected override IDictionary<string, string?> Settings
    {
        get
        {
            var settings = base.Settings;
            settings["ConnectionStrings:DefaultConnection"] = _postgres.GetConnectionString();
            settings["Database:ApplyMigrationsOnStartup"] = "true";
            settings["Database:SeedOnStartup"] = "true";
            settings["Seed:Auth:AdminEmail"] = AdminEmail;
            settings["Seed:Auth:AdminPassword"] = AdminPassword;
            settings["Seed:Auth:AuthorPassword"] = AuthorPassword;
            return settings;
        }
    }

    public Task InitializeAsync() => _postgres.StartAsync();

    async Task IAsyncLifetime.DisposeAsync()
    {
        await base.DisposeAsync();
        await _postgres.DisposeAsync();
    }
}
