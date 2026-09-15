using System.Text.RegularExpressions;
using CulinaryBlog.Application.Auth.Dtos;
using CulinaryBlog.Application.Auth.Mappers;
using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Common.Interfaces;
using CulinaryBlog.Domain.Constants;
using CulinaryBlog.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace CulinaryBlog.Application.Auth.GoogleLogin;

public partial class GoogleLoginCommandHandler : IRequestHandler<GoogleLoginCommand, AuthResponseDto>
{
    private const string GoogleProvider = "Google";
    private static readonly TimeSpan RefreshTokenLifetime = TimeSpan.FromDays(7);

    private readonly IGoogleTokenValidator _googleTokenValidator;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IApplicationDbContext _dbContext;
    private readonly IJwtService _jwtService;

    public GoogleLoginCommandHandler(
        IGoogleTokenValidator googleTokenValidator,
        UserManager<ApplicationUser> userManager,
        IApplicationDbContext dbContext,
        IJwtService jwtService)
    {
        _googleTokenValidator = googleTokenValidator;
        _userManager = userManager;
        _dbContext = dbContext;
        _jwtService = jwtService;
    }

    public async Task<AuthResponseDto> Handle(GoogleLoginCommand request, CancellationToken cancellationToken)
    {
        var profile = await _googleTokenValidator.ValidateAsync(request.IdToken, cancellationToken);

        if (string.IsNullOrWhiteSpace(profile.Email) || !profile.EmailVerified)
        {
            throw new BadRequestException("Tài khoản Google thiếu email đã xác thực.");
        }

        var user = await _userManager.FindByLoginAsync(GoogleProvider, profile.ProviderKey);

        if (user is null)
        {
            user = await _userManager.FindByEmailAsync(profile.Email);

            if (user is null)
            {
                user = ApplicationUser.Create(
                    profile.FullName ?? profile.Email,
                    profile.Email,
                    await GenerateUniqueUserNameAsync(profile.Email));

                var createResult = await _userManager.CreateAsync(user);
                if (!createResult.Succeeded)
                {
                    throw new FluentValidation.ValidationException(createResult.Errors.Select(e =>
                        new FluentValidation.Results.ValidationFailure(nameof(profile.Email), e.Description)));
                }

                await _userManager.AddToRoleAsync(user, Roles.Author);
            }

            var loginResult = await _userManager.AddLoginAsync(
                user, new UserLoginInfo(GoogleProvider, profile.ProviderKey, GoogleProvider));

            if (!loginResult.Succeeded)
            {
                throw new ConflictException("Không thể liên kết tài khoản Google với người dùng hiện có.");
            }
        }

        var roles = await _userManager.GetRolesAsync(user);

        var accessToken = _jwtService.GenerateAccessToken(user, roles);
        var rawRefreshToken = _jwtService.GenerateRefreshTokenValue();
        var refreshTokenHash = _jwtService.HashRefreshToken(rawRefreshToken);

        var refreshToken = RefreshToken.Create(user.Id, refreshTokenHash, RefreshTokenLifetime, request.IpAddress);
        _dbContext.RefreshTokens.Add(refreshToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new AuthResponseDto(
            accessToken.Token,
            rawRefreshToken,
            accessToken.ExpiresAt,
            UserProfileMapper.ToDto(user, roles));
    }

    private async Task<string> GenerateUniqueUserNameAsync(string email)
    {
        var localPart = email[..email.IndexOf('@')];
        var baseUserName = InvalidUserNameCharsRegex().Replace(localPart, "");
        if (string.IsNullOrWhiteSpace(baseUserName))
        {
            baseUserName = "user";
        }

        var candidate = baseUserName;
        var suffix = 1;
        while (await _userManager.FindByNameAsync(candidate) is not null)
        {
            candidate = $"{baseUserName}{suffix++}";
        }

        return candidate;
    }

    [GeneratedRegex("[^a-zA-Z0-9_.]")]
    private static partial Regex InvalidUserNameCharsRegex();
}
