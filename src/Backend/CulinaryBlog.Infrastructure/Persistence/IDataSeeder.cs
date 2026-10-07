namespace CulinaryBlog.Infrastructure.Persistence;

/// <summary>Dữ liệu mẫu của một module; được chạy sau khi migrate nếu bật Database:SeedOnStartup.</summary>
public interface IDataSeeder
{
    Task SeedAsync(CancellationToken cancellationToken);
}
