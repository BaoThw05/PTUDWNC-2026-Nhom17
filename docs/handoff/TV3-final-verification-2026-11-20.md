# Kiểm tra bàn giao TV3 — 20/11/2026

## Phiên bản kiểm tra

- Base `upstream/main`: `819d129` (chứa PR #77 và #78).
- Worktree: `CulinaryBlog-324-final`, nhánh `fix/infra-backup-nginx`.
- Stack `tv3final`, origin `http://localhost:18100`; database, network và volume
  riêng. Các stack và dữ liệu cũ được giữ nguyên.
- Issue [#79](https://github.com/BaoThw05/PTUDWNC-2026-Nhom17/issues/79):
  phục hồi định tuyến Nginx sau backup và cập nhật bằng chứng bàn giao.

## Kết quả chạy mới

| Kiểm tra | Kết quả |
|---|---|
| Backend build Release | 0 lỗi, 0 warning |
| Unit test | 209/209 đạt |
| Architecture test | 7/7 đạt |
| Integration test với Docker thật | 120/120 đạt, không bỏ qua |
| `dotnet format --verify-no-changes --no-restore` | Đạt |
| Frontend lint | Đạt, không lỗi hoặc warning |
| Frontend production build trong Docker | Đạt |
| Frontend E2E dùng mock API | 5/5 đạt, gồm tạo/publish/xem recipe |
| Build container API/web, khởi động Compose riêng | Đạt |
| `nginx -t` | Đạt |
| OpenAPI `/openapi/v1.json` | 200, 36 path; có endpoint category và ảnh |
| Smoke API/Nginx local | 36/36 đạt |
| Smoke API/Nginx HTTPS qua Quick Tunnel | 36/36 đạt |
| Hồi quy trình duyệt local và HTTPS | Anonymous 401, Bearer API, cookie đọc/upload/xóa ảnh đều đạt |
| Backup bằng script đã sửa | Exit 0; reload Nginx thành công |
| Truy cập sau backup | OpenAPI 200, ảnh qua `/media` đọc được |
| Restore sang project/volume mới | Exit 0; khớp 53 recipe, 54 ảnh, 10 danh mục |
| Kiểm tra nội dung ảnh sau restore | 6 SHA-256 khớp: ảnh gốc, medium và thumbnail của 2 ảnh |

Smoke gồm JPEG/PNG/WebP, resize medium rộng 800 và thumbnail 300×300,
đổi/xóa ảnh chính, giữ đúng một ảnh chính, slug không đổi khi đổi tên,
chặn xóa danh mục chứa recipe cả trong thùng rác và các lỗi 403/409/413/415.
Hangfire yêu cầu Basic Auth, truy cập có xác thực đạt.

Lỗi E2E `getByLabel("Email")` trong báo cáo 17/11 không còn tái hiện trên
`819d129`; cả 5 bài hiện đạt. Build Next.js vẫn có cảnh báo `metadataBase`
cho ảnh Open Graph; đây là phần SEO của TV4, không làm build thất bại.

## Lỗi sau backup và cách sửa

Backup dừng web/API/MinIO rồi bật lại. Docker có thể cấp IP mới trong network,
trong khi Nginx còn cache IP cũ. Lượt đầu tái hiện 502 khi đọc OpenAPI và gọi
API qua Nginx; reload Nginx khôi phục truy cập.

`scripts/backup.ps1` kiểm tra Nginx có chạy trước backup hay không. Sau khi
bật lại các service vốn đang chạy, script reload Nginx nếu trước đó đang chạy;
không tự bật Nginx đã dừng và không yêu cầu service này ở Compose chỉ có base.
Lượt backup sau sửa trả exit 0, OpenAPI phục hồi 200 và đối chiếu ảnh đạt.

Restore đầu tiên bị chặn đúng do đã khởi động MinIO trước, khiến volume có
metadata. Lượt chốt dùng project mới `tv3finalrestore3`, chỉ tạo container
MinIO trước restore, giữ volume trống; sau restore mới khởi động MinIO.
Không dùng `-ReplaceExisting` và không khôi phục đè dữ liệu cũ.

Các script PowerShell được thử qua bản sao UTF-8 BOM trong cùng thư mục
`scripts/` để Windows PowerShell đọc đúng tiếng Việt, rồi xóa bản sao.
Backup, cấu hình resolve, kết quả chi tiết và tài khoản kiểm tra giữ local,
không commit vào Git.

## Chốt 3.23–3.24

- PR #77 đã được owner merge ngày 19/11; issue #76 đã đóng completed.
- 3.23: tổng duyệt kỹ thuật local/HTTPS đạt. Chưa có xác nhận của thành viên
  bằng thiết bị và mạng độc lập; cần ghi người thử, thời điểm và kết quả.
- 3.24: bản sửa backup cần owner review/merge. Tag remote `v1.0` vẫn trỏ tới
  `ab3c2e2` ngày 27/10, chưa chứa bản bàn giao hiện tại; không di chuyển tag.
- Owner chọn commit cuối sau khi PR được merge và CI xanh, rồi thống nhất
  tên tag mới với nhóm; chưa đánh dấu hoàn tất toàn bộ.

Link Quick Tunnel chỉ có hiệu lực khi tiến trình demo đang chạy, không phải
hostname bàn giao cố định. Phương án dự phòng là demo bằng `localhost:18100`;
khi chuyển về local phải đổi `AUTH_URL` và reload Nginx sau khi recreate web.
