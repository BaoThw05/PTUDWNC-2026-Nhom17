using CulinaryBlog.Application.Common.Errors;
using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Common.Models;
using CulinaryBlog.Application.Features.Auth;
using CulinaryBlog.Application.Features.Auth.Abstractions;
using CulinaryBlog.Domain.Auth;
using CulinaryBlog.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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

    public async Task<string> CreatePasswordResetTokenAsync(Guid userId, CancellationToken cancellationToken) =>
        await userManager.GeneratePasswordResetTokenAsync(await GetRequiredAsync(userId));

    public async Task ResetPasswordAsync(Guid userId, string token, string newPassword, CancellationToken cancellationToken)
    {
        var result = await userManager.ResetPasswordAsync(await GetRequiredAsync(userId), token, newPassword);

        if (result.Errors.Any(error => error.Code == nameof(IdentityErrorDescriber.InvalidToken)))
        {
            throw new ValidationException(AuthErrorCodes.ResetTokenInvalid, "The password reset link is invalid or has expired.");
        }

        EnsureSucceeded(result);
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

    public async Task<PagedResult<UserAccount>> ListAsync(
        string? search,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var query = dbContext.Users.AsNoTracking();
        if (search is not null)
        {
            var term = search.ToLower();
            query = query.Where(user =>
                user.Email!.ToLower().Contains(term)
                || user.UserName!.ToLower().Contains(term)
                || user.FullName.ToLower().Contains(term));
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var users = await query
            .OrderByDescending(user => user.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var userIds = users.Select(user => user.Id).ToArray();
        var roles = (await (
                from userRole in dbContext.UserRoles
                join role in dbContext.Roles on userRole.RoleId equals role.Id
                where userIds.Contains(userRole.UserId)
                select new { userRole.UserId, role.Name })
            .ToListAsync(cancellationToken))
            .ToLookup(row => row.UserId, row => row.Name!);

        return new PagedResult<UserAccount>(
            [.. users.Select(user => ToAccount(user, [.. roles[user.Id].Order()]))],
            page,
            pageSize,
            totalCount);
    }

    public async Task<UserAccount> UpdateAccessAsync(
        Guid userId,
        bool? isActive,
        IReadOnlyList<string>? roles,
        CancellationToken cancellationToken)
    {
        var user = await GetRequiredAsync(userId);

        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        if (isActive is { } active && user.IsActive != active)
        {
            user.IsActive = active;
            EnsureSucceeded(await userManager.UpdateAsync(user));
        }

        if (roles is not null)
        {
            var current = await userManager.GetRolesAsync(user);
            var removed = current.Except(roles).ToArray();
            var added = roles.Except(current).ToArray();

            if (removed.Length > 0)
            {
                EnsureSucceeded(await userManager.RemoveFromRolesAsync(user, removed));
            }

            if (added.Length > 0)
            {
                EnsureSucceeded(await userManager.AddToRolesAsync(user, added));
            }
        }

        await transaction.CommitAsync(cancellationToken);

        return await ToAccountAsync(user);
    }

    private async Task<ApplicationUser> GetRequiredAsync(Guid userId) =>
        await userManager.FindByIdAsync(userId.ToString())
        ?? throw new NotFoundException("The user no longer exists.", AuthErrorCodes.UserNotFound);

    private async Task<UserAccount> ToAccountAsync(ApplicationUser user) =>
        ToAccount(user, [.. await userManager.GetRolesAsync(user)]);

    private static UserAccount ToAccount(ApplicationUser user, IReadOnlyList<string> roles) =>
        new(
            user.Id,
            user.Email ?? string.Empty,
            user.UserName ?? string.Empty,
            user.FullName,
            user.AvatarUrl,
            user.IsActive,
            user.CreatedAt,
            roles);

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
