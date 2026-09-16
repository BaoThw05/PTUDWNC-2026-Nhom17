using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Features.Auth;
using CulinaryBlog.Application.Features.Auth.Abstractions;
using Google.Apis.Auth;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CulinaryBlog.Infrastructure.Auth;

internal sealed class GoogleIdTokenValidator(
    IOptions<GoogleAuthOptions> options,
    ILogger<GoogleIdTokenValidator> logger) : IGoogleIdTokenValidator
{
    public async Task<GoogleIdentity> ValidateAsync(string idToken, CancellationToken cancellationToken)
    {
        var clientId = options.Value.ClientId;
        if (string.IsNullOrWhiteSpace(clientId))
        {
            logger.LogWarning("Google sign-in was requested but Authentication:Google:ClientId is not configured");
            throw Unavailable();
        }

        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            var payload = await GoogleJsonWebSignature.ValidateAsync(
                idToken,
                new GoogleJsonWebSignature.ValidationSettings { Audience = [clientId] });

            return new GoogleIdentity(payload.Subject, payload.Email, payload.EmailVerified, payload.Name, payload.Picture);
        }
        catch (InvalidJwtException)
        {
            throw new UnauthorizedException("The Google token is invalid.", AuthErrorCodes.GoogleTokenInvalid);
        }
        catch (HttpRequestException exception)
        {
            logger.LogWarning(exception, "Could not reach Google to validate the id token");
            throw Unavailable();
        }
    }

    private static ExternalServiceException Unavailable() =>
        new(AuthErrorCodes.GoogleUnavailable, "Google sign-in is currently unavailable.");
}
