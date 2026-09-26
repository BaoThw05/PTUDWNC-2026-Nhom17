# Abstractions

Interface mà tầng Application cần, phần cài đặt nằm ở Infrastructure. Khung gốc **chưa tạo** các interface dưới đây; người phụ trách tự thêm khi làm việc của mình.

| Interface | Phụ trách | Mã việc | Ghi chú |
|---|---|---|---|
| `ICurrentUser` | TV1 | 1.07 | `UserId`, vai trò… lấy từ JWT |
| `IFileStorage` | TV3 | 3.05 | Có `DeleteByPrefixAsync`; cài đặt S3 + Local |
| `IBackgroundJobService` | TV3 | 3.07 | Bọc Hangfire; cung cấp enqueue, schedule và recurring job để Application không phụ thuộc Hangfire |
| `ICacheInvalidator` | TV4 | 4.06 | Bản no-op trước, Output Cache theo tag sau |

Quy ước: mỗi interface một file, namespace `CulinaryBlog.Application.Abstractions`, mọi method I/O là `async` và nhận `CancellationToken`.
