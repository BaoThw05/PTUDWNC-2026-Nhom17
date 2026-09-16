using System.Security.Cryptography;
using System.Text;

namespace CulinaryBlog.Application.Features.Auth.Common;

/// <summary>Sinh và băm refresh token (S-06): 32 byte ngẫu nhiên dạng base64url, lưu SHA-256 dạng hex.</summary>
public static class RefreshTokenValue
{
    private const int TokenBytes = 32;

    public static string Generate() => Base64UrlEncode(RandomNumberGenerator.GetBytes(TokenBytes));

    public static string Hash(string token) =>
        Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(token)));

    private static string Base64UrlEncode(byte[] bytes) =>
        Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
}
