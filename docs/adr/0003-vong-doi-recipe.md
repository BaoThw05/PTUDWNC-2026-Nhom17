# ADR-0003: Vòng đời trạng thái Recipe (Draft/Published/Archived)

## Trạng thái
Đã chấp thuận — 20/09/2026

## Bối cảnh (Context)
Recipe cần trải qua các trạng thái xuất bản khác nhau, và cần điều kiện tối thiểu để công khai,
tránh xuất bản công thức thiếu nội dung (ví dụ chưa có bước nào).

## Quyết định (Decision)
- Trạng thái: `Draft → Published`, `Published → Draft`, `Draft/Published → Archived`, `Archived → Draft`.
- Điều kiện để `Published`: **≥ 1 bước thực hiện** (`RecipeStep`). Nguyên liệu và ảnh **không bắt buộc**,
  có thể bổ sung sau khi đã publish (theo phản hồi giảng viên).
- `PublishedAt` được set ở **lần publish đầu tiên** và giữ nguyên cho các lần publish lại sau đó.
- Gọi API chuyển sang đúng trạng thái đích hiện tại (ví dụ Published gọi lại publish) → trả **200 OK** (idempotent),
  không báo lỗi.
- Xóa bước cuối cùng của 1 recipe đang Published → chặn lại, trả **422** mã lỗi `RECIPE_PUBLISH_INCOMPLETE`.
- Endpoint công khai (`GET /recipes/{slug}`) chỉ trả về Recipe có trạng thái Published và chưa bị xóa.
  Recipe Draft của người khác khi truy cập trực tiếp → trả **404** (không tiết lộ sự tồn tại).
- Slug: tự động thêm hậu tố `-2`, `-3`... khi trùng. Sinh lại slug khi đổi tiêu đề **chỉ khi recipe chưa từng
  được publish** (tránh đổi URL của bài đã công khai, ảnh hưởng SEO).

## Phương án khác đã cân nhắc
- **Publish không cần điều kiện gì**: đơn giản nhất, nhưng cho phép công thức trống nội dung hiển thị công khai.
- **Yêu cầu cả ảnh và nguyên liệu mới được publish**: chặt chẽ hơn nhưng giảng viên đã phản hồi không bắt buộc.

## Hệ quả (Consequences)
- Cần validate điều kiện "≥ 1 bước" ở cả lệnh Publish và lệnh Xóa Step.
- Cần xử lý slug sinh lại có điều kiện (kiểm tra `PublishedAt == null` trước khi sinh lại).