# ADR-002: Kiểm soát tương tranh (Optimistic Concurrency Control) bằng xmin

## Trạng thái
Đã chấp thuận — 20/09/2026 (TV2 Bùi Ngọc Toàn)

## Bối cảnh (Context)
Nhiều người hoặc nhiều tab có thể sửa cùng một Recipe đồng thời (vấn đề C-01, C-28, B-07). Cần cơ chế phát hiện xung đột để người sửa sau không ghi đè âm thầm lên dữ liệu của người sửa trước.
Do PostgreSQL không hỗ trợ kiểu `rowversion` / `bytea` tự tăng như SQL Server, cần lựa chọn cơ chế phù hợp cho PostgreSQL 16.

## Quyết định (Decision)
- Dùng cột hệ thống `xmin` có sẵn của PostgreSQL, ánh xạ vào property `uint Version` trên `BaseEntity`, cấu hình `IsRowVersion()` trong EF Core (`AppDbContext`).
- `GET /recipes/{id}` trả về `version` trong response body.
- `PUT /recipes/{id}` **bắt buộc** client gửi lại đúng `version` đã nhận.
- Handler gán `db.SetOriginalVersion(recipe, request.Version)`. Nếu `version` client gửi lệch với giá trị `xmin` hiện tại trong DB, EF Core ném `DbUpdateConcurrencyException`.
- `GlobalExceptionHandler` bắt `DbUpdateConcurrencyException` và map thành HTTP **409 Conflict** kèm mã lỗi `CONCURRENCY_CONFLICT` (hoặc `RECIPE_CONCURRENCY_CONFLICT`).
- **Cập nhật gián tiếp (C-28):** Mọi thao tác trên bảng con (thêm/sửa/xóa Step, Ingredient, Image) phải cập nhật `Recipe.UpdatedAt = DateTimeOffset.UtcNow` trong cùng transaction để PostgreSQL tăng `xmin` của Recipe cha.

## Phương án khác đã cân nhắc
- **Tự thêm cột `RowVersion` riêng:** không tận dụng được cơ chế có sẵn của PostgreSQL, phải tự tay quản lý giá trị mỗi lần ghi.
- **Pessimistic Locking (`SELECT FOR UPDATE`):** gây khóa bảng, giảm throughput khi lượng truy cập cao.

## Hệ quả (Consequences)
- API cập nhật phải nhận `version` và trả về `409` khi xung đột.
- Frontend nhận 409 sẽ thông báo bài viết đã có người sửa và yêu cầu tải lại dữ liệu mới nhất.
