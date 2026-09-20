# ADR-0001: Xóa dữ liệu Recipe — Phương án lai (Soft Delete + Thùng rác 30 ngày)

## Trạng thái
Đã chấp thuận — 20/09/2026

## Bối cảnh (Context)
SRS v1.0 không quy định rõ hành vi xóa Recipe: xóa thật ngay lập tức hay giữ lại để khôi phục.
Cần quyết định để tránh mất dữ liệu ngoài ý muốn, đồng thời không làm phình database vô thời hạn.

## Quyết định (Decision)
Áp dụng **Phương án C (lai)**:
- `Recipe` có `IsDeleted` (bool) + `DeletedAt` (DateTime?). Global Query Filter chỉ áp dụng cho `Recipe`.
- Bảng con (`RecipeStep`, `RecipeIngredient`, ảnh) và `Category` **xóa thật** (hard delete), không cần khôi phục.
- `DELETE /recipes/{id}` → chuyển vào thùng rác (ẩn khỏi trang công khai, xóa cache liên quan).
- `POST /recipes/{id}/restore` → khôi phục về đúng trạng thái trước khi xóa (Draft/Published/Archived). Nếu slug đã bị recipe khác chiếm trong lúc chờ khôi phục → tự thêm hậu tố.
- Recurring job (Hangfire) chạy định kỳ, xóa thật các recipe nằm trong thùng rác **quá 30 ngày**, đồng thời xóa ảnh liên quan theo prefix storage.
- Index unique trên `Slug` là **partial unique index** với điều kiện `WHERE "IsDeleted" = false` — cho phép nhiều bản ghi đã xóa trùng slug với nhau (nhưng không trùng với bản ghi đang hoạt động).

## Phương án khác đã cân nhắc
- **Xóa thật ngay lập tức**: đơn giản nhưng rủi ro mất dữ liệu do thao tác nhầm, không có cách khôi phục.
- **Chỉ soft delete, không có job dọn dẹp**: an toàn nhưng làm phình database vô thời hạn.

## Hệ quả (Consequences)
- Cần thêm 1 recurring job trong Hangfire.
- Cần `ICacheInvalidator` được gọi đúng lúc xóa/khôi phục để tránh cache stale.
- Query công khai (danh sách, tìm kiếm) phải luôn áp dụng Global Query Filter `IsDeleted = false`.