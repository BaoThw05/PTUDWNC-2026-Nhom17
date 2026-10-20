namespace CulinaryBlog.Infrastructure.Email;

public sealed class EmailOptions
{
    public const string SectionName = "Smtp";

    public string Host { get; set; } = string.Empty;

    public int Port { get; set; }

    public string SenderEmail { get; set; } = string.Empty;

    public string SenderName { get; set; } = string.Empty;

    public string? Username { get; set; }

    public string? Password { get; set; }
}
