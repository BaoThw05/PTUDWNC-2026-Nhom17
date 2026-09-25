using System.Security.Cryptography;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CulinaryBlog.Infrastructure.Auth;

/// <summary>
/// Ở môi trường Development, nếu chưa cấu hình khóa ký thì sinh khóa tạm để ai clone repo về cũng chạy được
/// mà không phải commit secret. Khóa đổi mỗi lần khởi động nên token cũ sẽ hết hiệu lực.
/// </summary>
internal sealed class JwtDevelopmentKeySetup : IPostConfigureOptions<JwtOptions>
{
    private const int GeneratedKeyBytes = 64;

    private readonly IHostEnvironment _environment;

    // IOptions và IOptionsMonitor giữ hai bản JwtOptions riêng; dùng chung một khóa để token ký và kiểm tra khớp nhau.
    private readonly Lazy<string> _temporaryKey;

    public JwtDevelopmentKeySetup(IHostEnvironment environment, ILogger<JwtDevelopmentKeySetup> logger)
    {
        _environment = environment;
        _temporaryKey = new Lazy<string>(() =>
        {
            logger.LogWarning(
                "Jwt:SigningKey is not configured; using a temporary key. Tokens become invalid when the API restarts");
            return Convert.ToBase64String(RandomNumberGenerator.GetBytes(GeneratedKeyBytes));
        });
    }

    public void PostConfigure(string? name, JwtOptions options)
    {
        if (string.IsNullOrEmpty(options.SigningKey) && _environment.IsDevelopment())
        {
            options.SigningKey = _temporaryKey.Value;
        }
    }
}
