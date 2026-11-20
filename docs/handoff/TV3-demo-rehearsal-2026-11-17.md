# Tổng duyệt TV3 — 17/11/2026

> Báo cáo lịch sử. Kết quả trên main mới và tiến độ 3.23–3.24 xem
> [kiểm tra ngày 20/11](TV3-final-verification-2026-11-20.md).

## Bản kiểm tra

- Nhánh: `fix/nginx-session-api`, tạo từ `upstream/main`.
- Commit: `0eefa75` (main cập nhật ngày 16/11/2026).
- Worktree riêng: `CulinaryBlog-323-rehearsal`.
- Stack kiểm tra riêng: `tv3rehearsal`, origin local `http://localhost:18090`.
- Database và volume riêng; không khôi phục đè dữ liệu của các stack hiện có.

## Kết quả kiểm tra

| Kiểm tra | Kết quả |
|---|---|
| Backend unit test Release | 195/195 đạt |
| Architecture test Release | 7/7 đạt |
| Integration test Release với Docker thật | 115/115 đạt, không bỏ qua test |
| `dotnet format --verify-no-changes --no-restore` | Đạt |
| Frontend production build trên Windows | Đạt |
| Frontend lint | 0 lỗi, 4 cảnh báo unused ở RecipeEditor/RecipeWizard (TV2) |
| Playwright hiện có (API giả lập) | 4/5 đạt; luồng Recipe thất bại khi tìm ô Email |
| Backend build Release | 0 lỗi, 0 warning |
| Build container API/web và stack riêng | Đạt |
| Smoke qua Nginx local | 36/36 kiểm tra đạt |
| Smoke API qua Quick Tunnel HTTPS | 36/36 kiểm tra đạt; chạy lại sau sửa Nginx cũng đạt |
| Trình duyệt qua Quick Tunnel | Đăng nhập, mở quản trị danh mục, đọc và upload ảnh bằng cookie phiên đạt sau sửa |
| `nginx -t` | Đạt |
| Hồi quy Nginx `session-routing.cjs` | Đạt: anonymous 401, Bearer API, cookie read/upload/delete |
| Backup/restore | Hai script trả exit 0 khi gọi trực tiếp bằng subprocess; restore vào project/volume mới |
| Dữ liệu sau restore | Snapshot đầu khớp 52 recipe, 54 ảnh, 10 danh mục; SHA-256 ảnh gốc/medium/thumb khớp nguồn |

Lượt integration đầu tiên thất bại do Docker chưa chạy. Đã khởi động Docker
Desktop, chạy lại đủ 115 test và đạt. Kết quả chốt dùng lượt chạy có Docker.

### Lỗi cần phối hợp TV1/TV2

`src/Frontend/tests/e2e/recipe.spec.ts:8` hết thời gian chờ tại
`getByLabel("Email")`. Trang đã chuyển đúng tới đăng nhập; snapshot có textbox
Email nhưng locator theo label không tìm được. Cần đối chiếu liên kết label/input
ở form đăng nhập và hợp đồng locator của bài test trước khi sửa. Chưa thay đổi
code thuộc module Auth hoặc Recipe.

Các bài Playwright này dùng mock API, không thay thế tổng duyệt trên stack thật
hoặc kiểm tra từ thiết bị ngoài mạng.

### Lỗi tích hợp đã sửa — issue #76

Auth mới giữ token trên server và gắn token từ cookie tại Route Handler
`app/api/v1/[...path]`. Nginx cũ gửi `/api/v1/*` thẳng tới ASP.NET, khiến giao diện
đã đăng nhập vẫn nhận 401 khi đọc/upload ảnh. Kiểm thử API bằng Bearer token
không phát hiện được lỗi này.

Nginx hiện chuyển request không có Bearer token tới Next.js; request có Bearer
token tiếp tục tới backend trực tiếp. Các route Auth.js, OpenAPI, `/media` và
Hangfire giữ nguyên. Không thay đổi module Auth hoặc Recipe.

Kiểm tra hồi quy bằng trình duyệt thật và API trên stack đang chạy:

```powershell
# Đặt DEMO_BASE_URL, DEMO_EMAIL, DEMO_PASSWORD trong môi trường tiến trình
# với tài khoản kiểm tra; không ghi mật khẩu vào file hoặc lịch sử lệnh.
node docker/nginx/tests/session-routing.cjs
```

Script yêu cầu dependencies frontend/Chromium của Playwright đã được cài,
tạo recipe kiểm tra, xác nhận anonymous 401, Bearer API và cookie read/upload/
delete, rồi xóa ảnh và đưa recipe kiểm tra vào thùng rác.

### Môi trường tổng duyệt

- Dùng Quick Tunnel tạm thời vì máy chưa có token/hostname cố định; cấu hình
  `AUTH_URL` theo URL HTTPS trong lúc thử trình duyệt.
- Đã dừng Quick Tunnel sau kiểm tra và trả `AUTH_URL` về local.
- Khi recreate container web để đổi `AUTH_URL`, recreate/reload Nginx sau đó
  để cập nhật địa chỉ upstream. Đã thực hiện trước lượt hồi quy local cuối.
- Các lần gọi URL công khai và trình duyệt đi qua Cloudflare đều thực hiện từ
  máy chạy demo. Chưa có xác nhận của thành viên dùng thiết bị/mạng độc lập.
- Backup được thử bằng bản sao UTF-8 BOM của script gốc để Windows PowerShell
  đọc đúng tiếng Việt; chỉ áp dụng ExecutionPolicy Bypass cho tiến trình thử,
  truyền `-EnvFile` rõ ràng. Đã xóa các bản sao script tạm.
- Bản backup, file cấu hình đã resolve, thông tin tài khoản kiểm tra và token
  phiên chỉ giữ local, không đưa vào commit.

## Tiến độ bàn giao đã đối chiếu GitHub

- 3.01–3.18: các PR nền tảng, danh mục, ảnh, resize, Nginx và Tunnel đã merge.
- 3.19: PR #61 merge ngày 15/11/2026.
- 3.20: issue #52 được owner đóng là completed; integration test hiện đạt.
- 3.21: PR #55 merge ngày 15/11/2026.
- 3.22: PR #57 merge ngày 15/11/2026.
- 3.23: tổng duyệt kỹ thuật qua local/Tunnel đã thực hiện; còn xác nhận của nhóm
  từ thiết bị ngoài mạng và xử lý lỗi E2E nêu trên.
- 3.24: chưa chốt. Tag `v1.0` ngày 27/10 không chứa các PR merge ngày 15/11.

## Điều kiện chốt 3.23–3.24

1. Stack mới chạy qua Nginx; kiểm tra OpenAPI và gọi API thật.
2. Thử đăng nhập, danh mục, upload JPEG/PNG/WebP, resize, đổi/xóa ảnh chính,
   xem ảnh qua `/media`; kiểm tra các lỗi 403/409/413/415.
3. Thử backup và restore trên project/volume kiểm tra riêng.
4. Tổng duyệt qua Tunnel bằng thiết bị ngoài mạng; ghi origin và kết quả,
   không ghi token hoặc thông tin đăng nhập vào tài liệu.
5. Phối hợp xử lý lỗi E2E thuộc TV1/TV2; owner chốt tag chứa bản bàn giao cuối.
