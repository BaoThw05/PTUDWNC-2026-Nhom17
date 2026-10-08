# ADR-005: Vòng đời trạng thái Recipe (Draft / Published / Archived)

## Trạng thái
Đã chấp thuận — 20/09/2026 (TV2 Bùi Ngọc Toàn)

## Bối cảnh (Context)
Recipe trải qua các giai đoạn từ soạn thảo đến công khai và lưu trữ (vấn đề A-02, A-03, A-04, S-11). Cần quy định rõ máy trạng thái và điều kiện tối thiểu để xuất bản, tránh công thức rỗng xuất hiện trên trang công khai.

## Quyết định (Decision)
- **Máy trạng thái:**
  - `Draft → Published`
  - `Published → Draft`
  - `Draft / Published → Archived`
  - `Archived → Draft`
- **Điều kiện để Published:**
  - Bắt buộc có **≥ 1 bước thực hiện** (`RecipeStep`).
  - Nguyên liệu (`RecipeIngredient`) và Ảnh **không bắt buộc** (theo phản hồi của giảng viên, có thể bổ sung sau khi đã publish).
- **Tính Idempotent:** Gọi API chuyển sang trạng thái mà Recipe đang có (ví dụ Published gọi lại publish) trả về **200 OK**, không báo lỗi.
- **Bảo vệ toàn vẹn bước:** Xóa bước cuối cùng của recipe đang `Published` → chặn lại và trả lỗi **422** kèm mã `RECIPE_PUBLISH_INCOMPLETE`.
- **Ngày xuất bản (`PublishedAt`):** Được gán ở lần publish đầu tiên (`PublishedAt ??= DateTimeOffset.UtcNow`) và **giữ nguyên** vĩnh viễn (kể cả khi unpublish rồi publish lại) để bảo toàn ngày bài viết phục vụ SEO.
- **Slug tiếng Việt:**
  - Loại bỏ dấu tiếng Việt chuẩn (`đ/Đ` → `d`, phân rã ký tự dấu).
  - Tự động sinh hậu tố `-2`, `-3`... khi bị trùng.
  - Khi đổi tiêu đề, chỉ sinh lại slug nếu recipe **chưa từng được publish** (`PublishedAt == null`). Khi đã từng publish, slug trở thành bất biến để tránh hỏng backlink và SEO.
  - Cấm sử dụng slug có giá trị `search` (tránh xung đột routing).

## Hệ quả (Consequences)
- Validation kiểm tra ≥ 1 bước được áp dụng ở cả `PublishRecipeHandler` và `DeleteRecipeStepHandler`.
- Endpoint công khai `GET /recipes/{slug}` chỉ trả bài có `Status == Published` và `!IsDeleted`.
