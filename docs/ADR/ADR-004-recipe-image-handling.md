# ADR-004: Quy ước lưu trữ & kiểm tra ảnh công thức (RecipeImage)

- **Ngày**: 20/09/2026
- **Trạng thái**: Đã chấp nhận (Accepted)
- **Người thực hiện**: Lương Đức Sang (TV3)
- **Liên quan**: FR-FILE, FR-RCP (ảnh công thức), lỗi D-18/D-22

## Bối cảnh

Cần chốt cách đặt tên, kiểm tra và lưu trữ ảnh công thức trước khi TV2 (phụ
trách Recipe) bắt đầu tham chiếu tới `RecipeImage` trong entity `Recipe`.

## Quyết định

1. **Định dạng cho phép**: chỉ JPEG, PNG, WebP.
2. **Kiểm tra thật, không tin đuôi file**: kiểm tra **chữ ký file (magic
   bytes)** ở vài byte đầu của nội dung upload, không dựa vào phần mở rộng
   (`.jpg`, `.png`...) hay `Content-Type` do client gửi lên — cả hai đều có
   thể bị giả mạo.
3. **Kích thước tối đa**: 5MB/file.
4. **Cấu trúc object key trong storage**:
   ```
   recipes/{recipeId}/{imageId}/original.{ext}
   ```
   Xóa ảnh (hoặc xóa cả recipe) thực hiện bằng xóa theo **prefix**
   `recipes/{recipeId}/{imageId}/`, không xóa từng key lẻ.
5. **Trong database chỉ lưu object key** (chuỗi ở trên), **không lưu URL đầy
   đủ** — URL được dựng lúc trả về response, dựa trên cấu hình `ServiceURL`
   hiện tại. Mục đích: đổi domain/provider lưu trữ sau này không cần chạy
   migration cập nhật dữ liệu cũ.
6. **Ảnh chính (Primary Image)**: mỗi Recipe có đúng 1 ảnh được đánh dấu
   `IsPrimary = true` tại một thời điểm; đổi ảnh chính là một thao tác
   transaction (bỏ cờ ảnh cũ, đặt cờ ảnh mới) để tránh có 2 ảnh chính hoặc
   không có ảnh chính nào cùng lúc.

## Hệ quả

- TV2 (Recipe) khi thiết kế entity `Recipe`/`RecipeImage` cần theo đúng cấu
  trúc key và quy ước Primary Image ở trên.
- Job resize ảnh (FR-JOB-002, thumbnail 300x300 và medium 800x600) sẽ ghi
  thêm các biến thể vào cùng thư mục `recipes/{recipeId}/{imageId}/`, ví dụ
  `thumbnail.{ext}`, `medium.{ext}` — vẫn nằm trong cùng prefix nên vẫn xóa
  gọn bằng một lệnh xóa theo prefix.
