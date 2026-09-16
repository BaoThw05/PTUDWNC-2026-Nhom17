namespace CulinaryBlog.Domain.Common;

public abstract class BaseEntity
{
    public Guid Id { get; set; } = Guid.CreateVersion7();

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }

    /// <summary>
    /// Phiên bản dòng cho optimistic concurrency (S-04), map vào cột hệ thống <c>xmin</c> của PostgreSQL.
    /// </summary>
    public uint Version { get; set; }
}
