namespace CulinaryBlog.Application.Features.Auth.Abstractions;

public interface IGoogleIdTokenValidator
{
    /// <summary>
    /// Xác minh chữ ký, issuer, audience, hạn dùng của id_token.
    /// Token sai → <c>UnauthorizedException</c>; Google không phản hồi → <c>ExternalServiceException</c>.
    /// </summary>
    Task<GoogleIdentity> ValidateAsync(string idToken, CancellationToken cancellationToken);
}
