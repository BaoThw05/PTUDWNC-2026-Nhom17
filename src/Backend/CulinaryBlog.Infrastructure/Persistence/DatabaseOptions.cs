namespace CulinaryBlog.Infrastructure.Persistence;

public sealed class DatabaseOptions
{
    public const string SectionName = "Database";

    public bool ApplyMigrationsOnStartup { get; set; }

    public bool SeedOnStartup { get; set; }
}
