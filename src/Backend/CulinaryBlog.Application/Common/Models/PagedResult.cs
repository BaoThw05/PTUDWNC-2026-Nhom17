namespace CulinaryBlog.Application.Common.Models;

/// <summary>
/// Kết quả phân trang dùng chung cho mọi API danh sách (S-10a).
/// </summary>
public sealed record PagedResult<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount)
{
    public const int DefaultPageSize = 12;
    public const int MaxPageSize = 50;

    public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);

    public bool HasNextPage => Page < TotalPages;

    public bool HasPreviousPage => Page > 1;
}
