using CulinaryBlog.Application.Common.Errors;
using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Features.Auth;
using CulinaryBlog.Application.Features.Auth.Abstractions;
using CulinaryBlog.Domain.Auth;
using CulinaryBlog.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using ValidationException = CulinaryBlog.Application.Common.Exceptions.ValidationException;

namespace CulinaryBlog.Infrastructure.Auth;

internal sealed class IdentityUserAccountService(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    AppDbContext dbContext,
    TimeProvider timeProvider) : IUserAccountService
{
    private static readonly string DuplicateEmailCode = nameof(IdentityErrorDescriber.DuplicateEmail);
    private static readonly string DuplicateUserNameCode = nameof(IdentityErrorDescriber.DuplicateUserName);

    public async Task<UserAccount?> FindByIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        return user is null ? null : await ToAccountAsync(user);
    }

    public async Task<UserAccount?> FindByEmailAsync(string email, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(email);
        return user is null ? null : await ToAccountAsync(user);
    }

    public async Task<UserAccount?> FindByExternalLoginAsync(ExternalLogin login, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByLoginAsync(login.Provider, login.ProviderKey);
        return user is null ? null : await ToAccountAsync(user);
    }

    public async Task<UserAccount> CreateAsync(NewUserAccount account, CancellationToken cancellationToken)
    {
        var user = new ApplicationUser
        {
            Id = Guid.CreateVersion7(),
            UserName = account.UserName,
            Email = account.Email,
            // Tài khoản không có mật khẩu chỉ được tạo từ Google, email đã được Google xác minh.
            EmailConfirmed = account.Password is null,
            FullName = account.FullName,
            AvatarUrl = account.AvatarUrl,
            CreatedAt = timeProvider.GetUtcNow(),
        };

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        EnsureSucceeded(account.Password is null
            ? await userManager.CreateAsync(user)
            : await userManager.CreateAsync(user, account.Password));
        EnsureSucceeded(await userManager.AddToRoleAsync(user, Roles.Author));

        await transaction.CommitAsync(cancellationToken);

        return await ToAccountAsync(user);
    }

    public async Task<PasswordCheckResult> CheckPasswordAsync(
        Guid userId,
        string password,
        CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            return PasswordCheckResult.InvalidPassword;
        }

        var result = await signInManager.CheckPasswordSignInAsync(user, password, lockoutOnFailure: true);

        return result switch
        {
            { Succeeded: true } => PasswordCheckResult.Success,
            { IsLockedOut: true } => PasswordCheckResult.LockedOut,
            _ => PasswordCheckResult.InvalidPassword,
        };
    }

    public async Task SetPasswordAsync(Guid userId, string newPassword, CancellationToken cancellationToken)
    {
        var user = await GetRequiredAsync(userId);
        var token = await userManager.GeneratePasswordResetTokenAsync(user);
        EnsureSucceeded(await userManager.ResetPasswordAsync(user, token, newPassword));
    }

    public async Task<UserAccount> UpdateFullNameAsync(
        Guid userId,
        string fullName,
        CancellationToken cancellationToken)
    {
        var user = await GetRequiredAsync(userId);
        user.FullName = fullName;
        EnsureSucceeded(await userManager.UpdateAsync(user));

        return await ToAccountAsync(user);
    }

    public async Task<UserAccount> LinkExternalLoginAsync(
        Guid userId,
        ExternalLogin login,
        string? avatarUrl,
        CancellationToken cancellationToken)
    {
        var user = await GetRequiredAsync(userId);

        EnsureSucceeded(await userManager.AddLoginAsync(
            user,
            new UserLoginInfo(login.Provider, login.ProviderKey, login.Provider)));

        if (avatarUrl is not null && user.AvatarUrl is null)
        {
            user.AvatarUrl = avatarUrl;
        }

        user.EmailConfirmed = true;
        EnsureSucceeded(await userManager.UpdateAsync(user));

        return await ToAccountAsync(user);
    }

    private async Task<ApplicationUser> GetRequiredAsync(Guid userId) =>
        await userManager.FindByIdAsync(userId.ToString())
        ?? throw new NotFoundException("The user no longer exists.", AuthErrorCodes.UserNotFound);

    private async Task<UserAccount> ToAccountAsync(ApplicationUser user)
    {
        var roles = await userManager.GetRolesAsync(user);

        return new UserAccount(
            user.Id,
            user.Email ?? string.Empty,
            user.UserName ?? string.Empty,
            user.FullName,
            user.AvatarUrl,
            user.IsActive,
            user.CreatedAt,
            [.. roles]);
    }

    private static void EnsureSucceeded(IdentityResult result)
    {
        if (result.Succeeded)
        {
            return;
        }

        if (result.Errors.Any(error => error.Code == DuplicateEmailCode))
        {
            throw new ConflictException("The email is already registered.", AuthErrorCodes.EmailExists);
        }

        if (result.Errors.Any(error => error.Code == DuplicateUserNameCode))
        {
            throw new ConflictException("The username is already taken.", AuthErrorCodes.UserNameExists);
        }

        var errors = result.Errors
            .GroupBy(error => error.Code.StartsWith("Password", StringComparison.Ordinal) ? "Password" : "Account")
            .ToDictionary(group => group.Key, group => group.Select(error => error.Description).ToArray());

        throw new ValidationException(errors);
    }
}
