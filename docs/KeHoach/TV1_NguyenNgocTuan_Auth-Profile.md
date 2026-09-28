# Kế hoạch cá nhân — TV1 · Nguyễn Ngọc Tuấn

> **Module:** Xác thực & Hồ sơ (FR-AUTH-001 → 007) · Email chào mừng (FR-JOB-001)
> **Vai trò chung:** điều phối tài liệu (SRS v1.1, ADR, bảng Change Request)
> **Hạn chốt:** CN 01/11/2026 — ứng dụng hoàn thiện (đăng nhập, giao diện đầy đủ, web gần như hoàn chỉnh) · Xem mốc chung trong `00_KeHoach_TongThe.md`

## 1. Phạm vi trách nhiệm

| Nhóm | Nội dung |
|---|---|
| Backend | ASP.NET Core Identity; `JwtService`; cấu hình JWT Bearer và policy dùng chung; `ICurrentUser`; seed tài khoản; các endpoint `/api/v1/auth/*`; bảng `RefreshTokens`; rate limiting và forwarded headers; `WelcomeEmailJob`; job dọn refresh token |
| Frontend | Auth.js v5 (Credentials, sau đó Google); middleware bảo vệ route; màn hình `/auth/login`, `/auth/register`, `/profile`; tự refresh token |
| Chung | Tổng hợp quyết định vào ADR và SRS v1.1; chốt lại hai điểm diễn giải với giảng viên |
| Lỗi SRS phụ trách xử lý | C-01, C-02, C-03, D-05, D-06, D-07, D-08, D-09, D-13, D-25, D-26, D-27, D-30, B-11, F-03, G-01, G-02, G-08 (và phần Auth của A-02, B-08, B-10, D-10, D-16) |

## 2. Quyết định áp dụng cho phần việc

- **Kiến trúc (S-05):** Auth.js v5 dạng BFF. Auth.js giữ phiên trong cookie HttpOnly; bên trong phiên có access token (15 phút) và refresh token của backend. **Dự phòng:** nếu spike thất bại → backend đặt refresh token trong cookie HttpOnly (phương án B).
- **Đăng ký:** body `{ email, password, displayName }`; không bắt buộc `userName` (Identity `UserName` = email); đăng ký xong tự đăng nhập.
- **Mật khẩu:** ≥ 8 ký tự, có chữ hoa, chữ thường, số, ký tự đặc biệt (NFR-SEC-001), cấu hình trong `IdentityOptions.Password`.
- **Đăng nhập:** `SignInManager.CheckPasswordSignInAsync(..., lockoutOnFailure: true)`; khóa 15 phút sau 5 lần sai → 423; `IsActive = false` → 403 `AUTH_ACCOUNT_DISABLED`; sai thông tin → 401 thông báo chung.
- **Refresh token (S-06):** 32 byte ngẫu nhiên, chỉ lưu SHA-256; bảng có `FamilyId`, `RevokedAt`, `RevokedReason`, `ReplacedByTokenHash`, `CreatedByIp`; xoay vòng mỗi lần refresh; token cũ dùng lại trong 30 giây → cấp mới; quá 30 giây → thu hồi cả family.
- **Logout:** `AllowAnonymous`, chỉ cần `refreshToken` trong body.
- **Google (S-05, mức Should):** backend chỉ nhận `{ idToken }`, tự xác minh bằng `Google.Apis.Auth`, kiểm tra `email_verified` trước khi liên kết tài khoản; `ClientSecret` chỉ nằm ở Next.js.
- **Rate limit (S-15):** `UseForwardedHeaders`; phân vùng theo userId/IP; login/register 5 lần/phút; refresh 30 lần/phút; chung 100 lần/phút; 429 kèm `Retry-After`.
- **Phạm vi (S-17):** không làm xác nhận email, đổi email/username bằng OTP; `AvatarUrl` chỉ lấy từ Google.
- **Mã lỗi:** `AUTH_EMAIL_EXISTS` (409), `AUTH_INVALID_CREDENTIALS` (401), `AUTH_ACCOUNT_LOCKED` (423), `AUTH_ACCOUNT_DISABLED` (403), `AUTH_REFRESH_TOKEN_EXPIRED` / `AUTH_REFRESH_TOKEN_REVOKED` (401), `AUTH_GOOGLE_TOKEN_INVALID` (401), `AUTH_GOOGLE_UNAVAILABLE` (502), `VALIDATION_ERROR` (422).

## 3. Mốc cá nhân

| Ngày | Mốc cá nhân | Gắn với mốc chung |
|---|---|---|
| CN 20/09 | ADR xác thực + SRS v1.1 khởi tạo | M0 |
| T4 23/09 | Kết luận spike Auth.js | — |
| T3 29/09 | Nhóm dùng được JWT + tài khoản seed | M1 |
| CN 11/10 | Toàn bộ API auth + email job | M2 |
| CN 18/10 | Màn hình auth chạy đầu-cuối | M3 |
| CN 25/10 | Rate limit, dọn token, Google | M4 |
| T6 30/10 | SRS v1.1 hoàn chỉnh | M6 |

## 4. Công việc chi tiết và deadline

**Mức ưu tiên:** M = Must (bắt buộc trước 01/11) · S = Should (cố gắng trước 01/11, trễ thì chuyển sau).

### G0 — Chốt quyết định (T4 16/09 → T3 22/09)

| Mã | Công việc | Kết quả bàn giao / tiêu chí xong | Phụ thuộc | Deadline | Mức |
|---|---|---|---|---|---|
| 1.01 | Gửi giảng viên xác nhận 2 điểm diễn giải (điều kiện publish; Cloudflare Tunnel không thay kho ảnh) | Có câu trả lời hoặc ghi "chấp nhận theo diễn giải của nhóm" | — | T7 19/09 | M |
| 1.02 | Tạo `docs/SRS_v1.1.md` (từ bản Markdown) + bảng Change Request + thư mục `docs/ADR/` + mẫu ADR | Cả nhóm mở được, có sẵn các dòng CR cho quyết định mục 3 của kế hoạch tổng | — | CN 20/09 | M |
| 1.03 | Viết ADR S-05 (kiến trúc xác thực) và S-06 (refresh token) | 2 file ADR, mỗi file ≤ 1 trang: bối cảnh, quyết định, hệ quả, phương án dự phòng | — | CN 20/09 | M |
| 1.04 | Bổ sung nhóm mã lỗi `AUTH_*` vào bảng mã lỗi chung | Bảng mã lỗi có đủ mã ở mục 2 | TV2 tạo bảng | CN 20/09 | M |

### G1 — Nền tảng (T4 23/09 → T4 30/09)

| Mã | Công việc | Kết quả bàn giao / tiêu chí xong | Phụ thuộc | Deadline | Mức |
|---|---|---|---|---|---|
| 1.05 | **Spike** Auth.js v5: Credentials provider gọi một endpoint login giả trên .NET; thử refresh trong callback `jwt` | Ghi kết luận vào ADR S-05: giữ phương án C hay chuyển B | — | T4 23/09 | M |
| 1.06 | Identity: `ApplicationUser` ở Infrastructure (`DisplayName`, `AvatarUrl`, `IsActive`, `CreatedAt`); `AddIdentityCore` + `AddSignInManager`; cấu hình mật khẩu và lockout; migration | Migration chạy trên PostgreSQL trong Docker; architecture test không báo Domain phụ thuộc Identity | TV2 khung solution (23/09), TV3 compose (24/09) | T7 26/09 | M |
| 1.07 | `JwtService` (HS256, `JsonWebTokenHandler`, claims `sub`, `email`, `role`, `jti`); cấu hình JWT Bearer dùng chung; policy `AuthorPolicy`, `AdminPolicy`; `ICurrentUser` | Endpoint thử `[AuthorPolicy]` trả 401/403/200 đúng | 1.06 | T2 28/09 | M |
| 1.08 | Seed Admin (role Admin + Author, mật khẩu lấy từ biến môi trường) và 3 Author test; `POST /auth/login` bản đầu (chưa có refresh) | **Bàn giao cho nhóm:** lấy được JWT qua Scalar để test endpoint cần đăng nhập | 1.07 | T3 29/09 | M |
| 1.09 | Khung Auth.js trên Next.js: Credentials provider, session chứa `accessToken`, middleware bảo vệ `/dashboard/*` và `/profile`, hàm lấy token cho API client | **Bàn giao cho TV2, TV3:** vào được trang dashboard khi đã đăng nhập | 1.05, TV4 khung Next.js (25/09) | T6 02/10 | M |

### G2a — Backend base (T5 01/10 → CN 11/10)

| Mã | Công việc | Kết quả bàn giao / tiêu chí xong | Phụ thuộc | Deadline | Mức |
|---|---|---|---|---|---|
| 1.10 | `POST /auth/register`: validator; transaction tạo user + gán role Author + phát refresh token; trùng email → 409; trả `AuthResponse` (tự đăng nhập) | Test: thành công, email trùng, mật khẩu yếu | 1.08 | T2 05/10 | M |
| 1.11 | `POST /auth/login` hoàn chỉnh: lockout, 423, `IsActive`, 401 thông báo chung | Test: đúng mật khẩu, sai mật khẩu, bị khóa sau 5 lần, tài khoản bị vô hiệu hóa | 1.10 | T3 06/10 | M |
| 1.12 | Bảng `RefreshTokens` + `POST /auth/refresh`: xoay vòng, ân hạn 30 giây, phát hiện dùng lại → thu hồi family | Test: refresh hợp lệ, hết hạn, dùng lại trong 30 giây, dùng lại sau 30 giây | 1.11 | T6 09/10 | M |
| 1.13 | `POST /auth/logout` (`AllowAnonymous`, thu hồi với lý do `Logout`) | Test: logout rồi refresh bằng token cũ → 401, không thu hồi family | 1.12 | T6 09/10 | M |
| 1.14 | `GET /auth/me`, `PATCH /auth/me` (`displayName` 2–100 ký tự) | Test: xem, sửa hợp lệ, sửa sai định dạng → 422 | 1.08 | T7 10/10 | M |
| 1.15 | `WelcomeEmailJob`: `IEmailService` (MailKit) → Mailpit; template HTML có tên người dùng và link ứng dụng; enqueue sau khi commit | Đăng ký xong thấy email trong Mailpit; job lỗi thì retry theo cấu hình chung | TV3 Hangfire (29/09) | CN 11/10 | M |

### G2b — Frontend base (T2 12/10 → CN 18/10)

| Mã | Công việc | Kết quả bàn giao / tiêu chí xong | Phụ thuộc | Deadline | Mức |
|---|---|---|---|---|---|
| 1.16 | Màn hình `/auth/register` và `/auth/login`: React Hook Form + Zod; hiển thị lỗi theo `code`; đã đăng nhập thì chuyển hướng; đăng ký xong gọi `signIn` | Đăng ký/đăng nhập được trên giao diện, lỗi hiển thị cạnh ô nhập | 1.09, 1.10, 1.11 | T4 14/10 | M |
| 1.17 | Màn hình `/profile` (xem/sửa tên hiển thị); refresh token trong callback `jwt`; lỗi refresh → đăng xuất và về trang login | Để phiên quá 15 phút vẫn thao tác được; thu hồi token thủ công → bị đưa về login | 1.12, 1.14 | T6 16/10 | M |
| 1.18 | Chạy thử luồng đầu-cuối cùng TV2 (đăng ký → đăng nhập → vào dashboard → tạo recipe) và sửa lỗi phía auth | Luồng M3 phần xác thực không còn lỗi chặn | TV2 dashboard | CN 18/10 | M |

### G3 — Tích hợp & base nâng cao (T2 19/10 → CN 25/10)

| Mã | Công việc | Kết quả bàn giao / tiêu chí xong | Phụ thuộc | Deadline | Mức |
|---|---|---|---|---|---|
| 1.19 | Rate limiting: `UseForwardedHeaders` (tin mạng Docker nội bộ); phân vùng user/IP; hạn mức theo endpoint; 429 + `Retry-After`; Next.js server chuyển tiếp `X-Forwarded-For` | Test: login lần thứ 6 trong 1 phút → 429; hai người dùng khác IP không ảnh hưởng nhau | TV3 Nginx (song song) | T4 21/10 | M |
| 1.20 | Recurring job dọn refresh token đã hết hạn quá 30 ngày | Job hiện trong Hangfire, chạy hằng ngày | TV3 Hangfire | T5 22/10 | M |
| 1.21 | Đăng nhập Google: `POST /auth/google { idToken }` (xác minh, `email_verified`, liên kết, sinh `userName`, gửi email chào mừng cho user mới) + Google provider trên Auth.js + nút đăng nhập | Đăng nhập bằng tài khoản Google thật; token giả → 401 | 1.12, 1.15 | CN 25/10 | S |

### G4 — Kiểm thử, tài liệu, tổng duyệt (T2 26/10 → CN 01/11)

| Mã | Công việc | Kết quả bàn giao / tiêu chí xong | Phụ thuộc | Deadline | Mức |
|---|---|---|---|---|---|
| 1.22 | Hoàn thiện integration test cho mọi endpoint auth; E2E Playwright luồng đăng ký + đăng nhập | CI xanh; E2E chạy được trên máy | TV4 Playwright (25/10) | T4 28/10 | M |
| 1.23 | Rà soát bảo mật: không log token/mật khẩu, secret không commit, token chỉ lưu hash, lockout + rate limit hoạt động, CORS/cookie đúng cấu hình | Checklist ở mục 7 đánh dấu đủ | 1.19 | T5 29/10 | M |
| 1.24 | Tổng hợp SRS v1.1: nhận phần của TV2–TV4 (hạn T5 29/10); tự viết lại 3.1, 8.1, phần `AUTH_*` của Phụ lục B; đóng các dòng Change Request | `docs/SRS_v1.1.md` hoàn chỉnh, không còn "hoặc" trong yêu cầu | TV2, TV3, TV4 | T6 30/10 | M |
| 1.25 | Tổng duyệt demo toàn nhóm (phần đăng ký/đăng nhập/hồ sơ) | Kịch bản demo chạy trơn tru qua Cloudflare Tunnel | TV3 Tunnel | T7 31/10 | M |
| 1.26 | Sửa lỗi cuối, gắn tag `v1.0` cùng nhóm | Tag trên `main` | — | CN 01/11 | M |

## 5. Bàn giao và nhận

| Hướng | Nội dung | Với ai | Hạn |
|---|---|---|---|
| **Nhận** | Khung solution 4 tầng | TV2 | T4 23/09 |
| **Nhận** | Docker Compose (PostgreSQL, Mailpit) | TV3 | T5 24/09 |
| **Nhận** | Khung Next.js + API client | TV4 | T6 25/09 |
| **Nhận** | Hangfire + `IBackgroundJobService` | TV3 | T3 29/09 |
| **Giao** | JWT dùng chung + user seed + `POST /auth/login` | TV2, TV3, TV4 | T3 29/09 |
| **Giao** | Auth.js session + middleware bảo vệ route | TV2, TV3 | T6 02/10 |
| **Nhận** | Playwright đã cài sẵn trong repo | TV4 | CN 25/10 |
| **Nhận** | Phần SRS v1.1 của từng module | TV2, TV3, TV4 | T5 29/10 |

## 6. Hướng phát triển sau 01/11

| Thứ tự | Hạng mục | Ghi chú |
|---|---|---|
| 1 | Đăng nhập Google (nếu 1.21 bị trễ) | Đã có thiết kế ở S-05 |
| 2 | Quên / đặt lại mật khẩu | Dùng token đặt lại mật khẩu của Identity + email qua Hangfire (tái sử dụng 1.15) |
| 3 | Gọi API qua Route Handler proxy của Next.js | Access token không còn xuống trình duyệt |
| 4 | Quản trị người dùng cho Admin | Danh sách, khóa/mở `IsActive`, gán role |
| 5 | Xác nhận email, đổi email bằng OTP | Đã ghi "ngoài phạm vi v1" trong SRS v1.1 |
| 6 | Xoay khóa ký JWT có `kid` | Hai khóa chạy song song trong thời gian chuyển |
| 7 | Nhật ký đăng nhập (thời gian, IP, thiết bị) | Hỗ trợ phát hiện bất thường |

## 7. Rủi ro, dự phòng và checklist

**Rủi ro:**

| Rủi ro | Dấu hiệu | Dự phòng |
|---|---|---|
| Auth.js v5 (beta) khó refresh ổn định | 1.05 quá T4 23/09 chưa chạy | Chuyển sang cookie HttpOnly từ backend; API backend không phải đổi |
| Bị đăng xuất ngẫu nhiên khi mở nhiều tab | Log "reuse detected" xuất hiện khi dùng bình thường | Kiểm tra lại thời gian ân hạn 30 giây, refresh sớm trước khi token hết hạn khoảng 1 phút |
| Việc tổng hợp tài liệu lấn thời gian code | 1.24 chưa nhận đủ phần của nhóm vào T5 29/10 | Nhắc nhóm trong kênh chung ngày CN 25/10; phần nào thiếu thì ghi CR "chưa cập nhật" thay vì tự viết hộ |
| Google Cloud Console cấu hình mất thời gian | 1.21 chưa xong T6 23/10 | Chuyển 1.21 sang sau 01/11 (mục đầu tiên trong danh sách cắt giảm) |

**Checklist tự kiểm tra trước CN 01/11:**
- [ ] Mật khẩu hash bằng Identity; bảng `RefreshTokens` chỉ có hash
- [ ] Không có token/mật khẩu trong log Seq
- [ ] Không có secret trong Git (JWT key, Google secret, SMTP)
- [ ] Sai mật khẩu 5 lần → khóa 15 phút; tài khoản `IsActive = false` không đăng nhập được
- [ ] Refresh xoay vòng đúng; dùng lại token cũ sau 30 giây → thu hồi family
- [ ] Logout xong refresh bằng token cũ → 401
- [ ] Rate limit trả 429 kèm `Retry-After`
- [ ] Mọi lỗi auth trả Problem Details có `code`
- [ ] Email chào mừng xuất hiện trong Mailpit
- [ ] SRS v1.1 phần 3.1 và 8.1 khớp với OpenAPI thực tế
