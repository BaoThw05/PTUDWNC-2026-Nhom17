namespace CulinaryBlog.Infrastructure.Auth;

/// <summary>Tài khoản mẫu cho môi trường dev (việc 1.08). Mật khẩu để trống thì không tạo tài khoản tương ứng.</summary>
public sealed class AuthSeedOptions
{
    public const string SectionName = "Seed:Auth";

    public string AdminEmail { get; set; } = "admin@culinaryblog.test";

    /// <summary>Không commit: đặt qua biến môi trường Seed__Auth__AdminPassword hoặc user-secrets.</summary>
    public string? AdminPassword { get; set; }

    public string[] AuthorEmails { get; set; } =
        ["author1@culinaryblog.test", "author2@culinaryblog.test", "author3@culinaryblog.test"];

    public string? AuthorPassword { get; set; }
}
