using CulinaryBlog.Application.Features.Auth.Abstractions;
using CulinaryBlog.Domain.Auth;

namespace CulinaryBlog.UnitTests.Features.Auth.Fakes;

internal sealed class InMemoryRefreshTokenRepository : IRefreshTokenRepository
{
    private readonly List<RefreshToken> _tokens = [];

    public IReadOnlyList<RefreshToken> Tokens => _tokens;

    public int SaveCount { get; private set; }

    /// <summary>Số lần tiếp theo TrySaveChangesAsync giả lập xung đột cập nhật đồng thời.</summary>
    public int ConflictsToSimulate { get; set; }

    public Task<RefreshToken?> FindByHashAsync(string tokenHash, CancellationToken cancellationToken) =>
        Task.FromResult(_tokens.SingleOrDefault(token => token.TokenHash == tokenHash));

    public Task<IReadOnlyList<RefreshToken>> GetActiveInFamilyAsync(
        Guid familyId,
        DateTimeOffset now,
        CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<RefreshToken>>(
            [.. _tokens.Where(token => token.FamilyId == familyId && token.IsActive(now))]);

    public void Add(RefreshToken token) => _tokens.Add(token);

    public Task<bool> TrySaveChangesAsync(CancellationToken cancellationToken)
    {
        if (ConflictsToSimulate > 0)
        {
            ConflictsToSimulate--;
            return Task.FromResult(false);
        }

        SaveCount++;
        return Task.FromResult(true);
    }
}
