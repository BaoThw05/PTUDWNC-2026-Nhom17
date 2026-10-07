using CulinaryBlog.Domain.Auth;

namespace CulinaryBlog.Application.Features.Auth.Abstractions;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> FindByHashAsync(string tokenHash, CancellationToken cancellationToken);

    Task<IReadOnlyList<RefreshToken>> GetActiveInFamilyAsync(
        Guid familyId,
        DateTimeOffset now,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<RefreshToken>> GetActiveForUserAsync(
        Guid userId,
        DateTimeOffset now,
        CancellationToken cancellationToken);

    void Add(RefreshToken token);

    /// <summary>Xóa vĩnh viễn các token có <c>ExpiresAt</c> trước <paramref name="cutoff"/>; trả về số dòng đã xóa (việc 1.20).</summary>
    Task<int> DeleteExpiredBeforeAsync(DateTimeOffset cutoff, CancellationToken cancellationToken);

    /// <summary>Trả về false khi token đã bị request khác sửa cùng lúc; khi đó các thay đổi đang chờ bị hủy.</summary>
    Task<bool> TrySaveChangesAsync(CancellationToken cancellationToken);
}
