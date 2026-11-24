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

    /// <summary>Gmail gửi song song với máy chủ chính (Mailpit). Bỏ trống Username/Password thì tắt.</summary>
    public GmailSmtpOptions Gmail { get; set; } = new();
}

public sealed class GmailSmtpOptions
{
    public string Host { get; set; } = "smtp.gmail.com";

    public int Port { get; set; } = 587;

    /// <summary>Địa chỉ Gmail gửi; cũng dùng làm địa chỉ From vì Gmail không cho From khác tài khoản.</summary>
    public string? Username { get; set; }

    /// <summary>App Password 16 ký tự, không phải mật khẩu đăng nhập.</summary>
    public string? Password { get; set; }

    public bool IsConfigured => !string.IsNullOrWhiteSpace(Username) && !string.IsNullOrWhiteSpace(Password);
}
