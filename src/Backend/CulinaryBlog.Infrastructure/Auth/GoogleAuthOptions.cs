namespace CulinaryBlog.Infrastructure.Auth;

public sealed class GoogleAuthOptions
{
    public const string SectionName = "Authentication:Google";

    /// <summary>OAuth Client ID (không phải secret). ClientSecret chỉ nằm ở Next.js (S-05).</summary>
    public string ClientId { get; set; } = string.Empty;
}
