using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace CulinaryBlog.Infrastructure.Auth;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";
    public const int MinSigningKeyBytes = 32;
    public const string RoleClaimType = "role";
    public const string NameClaimType = "name";

    public string Issuer { get; set; } = "CulinaryBlog";

    public string Audience { get; set; } = "CulinaryBlog.Web";

    /// <summary>Khóa HS256, tối thiểu 32 byte. Không commit: đặt qua biến môi trường Jwt__SigningKey hoặc user-secrets.</summary>
    public string SigningKey { get; set; } = string.Empty;

    /// <summary>
    /// Khóa cũ vẫn được chấp nhận khi kiểm tra token (xoay khóa): chuyển khóa đang dùng vào đây, đặt khóa mới cho
    /// <see cref="SigningKey"/>, và xóa khóa cũ sau khi token cuối cùng ký bằng nó hết hạn (<see cref="AccessTokenLifetimeMinutes"/>).
    /// </summary>
    public string[] PreviousSigningKeys { get; set; } = [];

    public int AccessTokenLifetimeMinutes { get; set; } = 15;

    public SymmetricSecurityKey CreateSigningKey() => CreateKey(SigningKey);

    public IEnumerable<SecurityKey> CreateValidationKeys() => [CreateSigningKey(), .. PreviousSigningKeys.Select(CreateKey)];

    // kid lấy từ SHA-256 của khóa nên không phải cấu hình riêng và không lộ khóa; handler dùng kid để chọn đúng khóa kiểm tra.
    private static SymmetricSecurityKey CreateKey(string key)
    {
        var bytes = Encoding.UTF8.GetBytes(key);
        return new SymmetricSecurityKey(bytes) { KeyId = Base64UrlEncoder.Encode(SHA256.HashData(bytes)[..8]) };
    }
}
