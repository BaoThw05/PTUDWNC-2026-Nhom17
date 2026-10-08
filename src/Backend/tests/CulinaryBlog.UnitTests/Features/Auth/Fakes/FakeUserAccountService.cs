using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Features.Auth;
using CulinaryBlog.Application.Features.Auth.Abstractions;
using CulinaryBlog.Domain.Auth;

namespace CulinaryBlog.UnitTests.Features.Auth.Fakes;

internal sealed class FakeUserAccountService : IUserAccountService
{
    private readonly Dictionary<Guid, UserAccount> _users = [];
    private readonly Dictionary<Guid, string?> _passwords = [];
    private readonly Dictionary<ExternalLogin, Guid> _logins = [];

    public HashSet<Guid> LockedOut { get; } = [];

    public UserAccount Add(string email, string password, bool isActive = true, string userName = "testuser")
    {
        var user = new UserAccount(
            Guid.NewGuid(),
            email,
            userName,
            "Test User",
            AvatarUrl: null,
            isActive,
            DateTimeOffset.UnixEpoch,
            [Roles.Author]);
        _users[user.Id] = user;
        _passwords[user.Id] = password;
        return user;
    }

    public void Replace(UserAccount user) => _users[user.Id] = user;

    public bool HasLogin(ExternalLogin login) => _logins.ContainsKey(login);

    public Task<UserAccount?> FindByIdAsync(Guid userId, CancellationToken cancellationToken) =>
        Task.FromResult(_users.GetValueOrDefault(userId));

    public Task<UserAccount?> FindByEmailAsync(string email, CancellationToken cancellationToken) =>
        Task.FromResult(_users.Values.SingleOrDefault(user =>
            string.Equals(user.Email, email, StringComparison.OrdinalIgnoreCase)));

    public Task<UserAccount?> FindByExternalLoginAsync(ExternalLogin login, CancellationToken cancellationToken) =>
        Task.FromResult(_logins.TryGetValue(login, out var userId) ? _users[userId] : null);

    public Task<UserAccount> CreateAsync(NewUserAccount account, CancellationToken cancellationToken)
    {
        if (_users.Values.Any(user => string.Equals(user.UserName, account.UserName, StringComparison.OrdinalIgnoreCase)))
        {
            throw new ConflictException("The username is already taken.", AuthErrorCodes.UserNameExists);
        }

        var user = new UserAccount(
            Guid.NewGuid(),
            account.Email,
            account.UserName,
            account.FullName,
            account.AvatarUrl,
            IsActive: true,
            DateTimeOffset.UnixEpoch,
            [Roles.Author]);
        _users[user.Id] = user;
        _passwords[user.Id] = account.Password;
        return Task.FromResult(user);
    }

    public Task<PasswordCheckResult> CheckPasswordAsync(Guid userId, string password, CancellationToken cancellationToken)
    {
        if (LockedOut.Contains(userId))
        {
            return Task.FromResult(PasswordCheckResult.LockedOut);
        }

        return Task.FromResult(_passwords.GetValueOrDefault(userId) == password
            ? PasswordCheckResult.Success
            : PasswordCheckResult.InvalidPassword);
    }

    public Task SetPasswordAsync(Guid userId, string newPassword, CancellationToken cancellationToken)
    {
        _passwords[userId] = newPassword;
        return Task.CompletedTask;
    }

    public Task<UserAccount> UpdateFullNameAsync(Guid userId, string fullName, CancellationToken cancellationToken)
    {
        var user = GetRequired(userId) with { FullName = fullName };
        _users[userId] = user;
        return Task.FromResult(user);
    }

    public Task<UserAccount> LinkExternalLoginAsync(
        Guid userId,
        ExternalLogin login,
        string? avatarUrl,
        CancellationToken cancellationToken)
    {
        var user = GetRequired(userId);
        _logins[login] = userId;
        if (avatarUrl is not null && user.AvatarUrl is null)
        {
            user = user with { AvatarUrl = avatarUrl };
            _users[userId] = user;
        }

        return Task.FromResult(user);
    }

    private UserAccount GetRequired(Guid userId) =>
        _users.GetValueOrDefault(userId) ?? throw new NotFoundException("User not found.");
}
