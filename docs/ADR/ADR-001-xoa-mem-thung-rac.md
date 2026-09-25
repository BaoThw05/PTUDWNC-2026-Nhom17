# ADR-001: Xóa dữ liệu Recipe — Phương án lai (Soft Delete + Thùng rác 30 ngày)

## Trạng thái
Đã chấp thuận — 20/09/2026 (TV2 Bùi Ngọc Toàn)

## Bối cảnh (Context)
SRS v1.0 không quy định rõ hành vi xóa Recipe: xóa thật ngay lập tức hay giữ lại để khôi phục (vấn đề A-01, A-07, C-21).
Cần quyết định dứt điểm để tránh mất dữ liệu ngoài ý muốn của người dùng, đồng thời không làm phình cơ sở dữ liệu vô thời hạn.

## Quyết định (Decision)
Áp dụng **Phương án C (mô hình lai)**:
- **`Recipe` (Aggregate Root):** Áp dụng **Soft Delete** (`IsDeleted` bool, `DeletedAt` DateTimeOffset?). Global Query Filter trong EF Core chỉ áp dụng cho `Recipe` (`!r.IsDeleted`).
- **Các Entity con (`RecipeStep`, `RecipeIngredient`, ảnh) và `Category`:** Áp dụng **Hard Delete**. Bảng con liên kết khóa ngoại với `OnDelete(DeleteBehavior.Cascade)`. Khi xóa thật Recipe cha, database tự động dọn sạch các bảng con.
- `DELETE /recipes/{id}` → chuyển vào thùng rác (ẩn khỏi trang công khai, xóa cache liên quan qua `ICacheInvalidator`).
- `POST /recipes/{id}/restore` → khôi phục về đúng trạng thái trước khi xóa (Draft/Published/Archived). Nếu slug đã bị recipe khác chiếm trong lúc chờ khôi phục → tự thêm hậu tố (`-2`, `-3`).
- Recurring job (Hangfire) chạy định kỳ, xóa thật các recipe nằm trong thùng rác **quá 30 ngày**, đồng thời xóa ảnh liên quan theo prefix storage.
- Index unique trên `Slug` là **partial unique index** với điều kiện `WHERE "IsDeleted" = false` — cho phép nhiều bản ghi đã xóa trùng slug với nhau mà không xung đột với bản ghi đang hoạt động.

## Phương án khác đã cân nhắc
- **Xóa thật ngay lập tức:** đơn giản nhưng rủi ro mất dữ liệu do thao tác nhầm, không có cách khôi phục.
- **Soft delete toàn bộ các bảng con:** làm hỏng cascade delete, logic tính `recipeCount` bị sai và truy vấn phức tạp.

## Hệ quả (Consequences)
- Query công khai (danh sách, tìm kiếm) luôn tự động áp dụng Global Query Filter `IsDeleted = false`.
- Query cá nhân thùng rác cần dùng `IgnoreQueryFilters()` thông qua `IAppDbContext.RecipesIncludingDeleted`.
