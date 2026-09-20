# ADR-0002: Kiểm soát tương tranh (Concurrency) bằng xmin

## Trạng thái
Đã chấp thuận — 20/09/2026

## Bối cảnh (Context)
Nhiều người có thể sửa cùng một Recipe đồng thời (ví dụ mở 2 tab). Cần cơ chế phát hiện xung đột,
tránh trường hợp người sửa sau ghi đè âm thầm lên thay đổi của người sửa trước.

## Quyết định (Decision)
- Dùng cột hệ thống `xmin` có sẵn của PostgreSQL, ánh xạ vào property `uint Version` trên `BaseEntity`,
  cấu hình `IsRowVersion()` trong EF Core.
- `GET /recipes/{id}` trả về `version` trong response.
- `PUT /recipes/{id}` **bắt buộc** client gửi lại đúng `version` đã nhận trước đó.
- Nếu `version` client gửi lệch với giá trị hiện tại trong DB → EF Core ném `DbUpdateConcurrencyException`,
  map thành lỗi **409 Conflict**, mã lỗi `RECIPE_CONCURRENCY_CONFLICT`.
- Mọi thao tác trên bảng con (thêm/sửa/xóa Step, Ingredient) phải cập nhật `Recipe.UpdatedAt`
  để `xmin` của Recipe cha cũng thay đổi theo — đảm bảo phát hiện được cả xung đột gián tiếp.

## Phương án khác đã cân nhắc
- **Tự thêm cột `RowVersion (byte[])` riêng** (kiểu SQL Server `rowversion`): không tận dụng được cơ chế
  có sẵn của PostgreSQL, phải tự tay cập nhật giá trị mỗi lần ghi.
- **Không kiểm soát concurrency**: đơn giản nhưng rủi ro mất dữ liệu khi ghi đè.

## Hệ quả (Consequences)
- Toàn bộ API cập nhật phải thiết kế để nhận và trả về `version`.
- Cần test riêng: sửa cùng 1 dòng từ 2 context khác nhau phải ném đúng exception.