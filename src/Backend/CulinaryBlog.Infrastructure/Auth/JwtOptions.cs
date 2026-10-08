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

    public int AccessTokenLifetimeMinutes { get; set; } = 15;

    public SymmetricSecurityKey CreateSigningKey() => new(Encoding.UTF8.GetBytes(SigningKey));
}
