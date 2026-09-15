using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Common.Interfaces;
using Google.Apis.Auth;
using Microsoft.Extensions.Options;

namespace CulinaryBlog.Infrastructure.Identity;

public class GoogleTokenValidator : IGoogleTokenValidator
{
    private readonly GoogleSettings _settings;

    public GoogleTokenValidator(IOptions<GoogleSettings> settings)
    {
        _settings = settings.Value;
    }

    public async Task<GoogleProfile> ValidateAsync(string idToken, CancellationToken cancellationToken = default)
    {
        GoogleJsonWebSignature.Payload payload;

        try
        {
            payload = await GoogleJsonWebSignature.ValidateAsync(idToken, new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = [_settings.ClientId],
            });
        }
        catch (InvalidJwtException ex)
        {
            // A1: Google token không hợp lệ hoặc hết hạn.
            throw new AppUnauthorizedException($"Google ID token không hợp lệ: {ex.Message}");
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            // A3: Google API không khả dụng.
            throw new ExternalServiceException("Không thể xác thực với Google lúc này.", ex);
        }

        return new GoogleProfile(
            ProviderKey: payload.Subject,
            Email: payload.Email,
            EmailVerified: payload.EmailVerified,
            FullName: payload.Name,
            AvatarUrl: payload.Picture);
    }
}
