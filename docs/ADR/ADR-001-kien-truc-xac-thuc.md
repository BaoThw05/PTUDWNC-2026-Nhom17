# ADR-001 · S-05 · Kiến trúc xác thực Next.js ↔ .NET

- **Trạng thái:** Đã chốt
- **Ngày:** 17/09/2026 (cập nhật 22/09/2026 — khớp lại hợp đồng đăng ký với `SRS_..._GiaiPhap.md` bản 19/09)
- **Người viết:** TV1
- **Liên quan:** C-01, C-02, C-03, D-05, D-06, D-08, D-09, F-03, G-08 · FR-AUTH-001 → 007 · CR-04, CR-06, CR-07

## Bối cảnh

SRS mô tả hai cách mâu thuẫn nhau: SPA tự giữ token (5.2) và Auth.js ở Next.js (6.1, FR-AUTH-003). Giữ refresh token 7 ngày trong `localStorage` dễ bị lấy mất qua XSS; trang dashboard cần biết người dùng đã đăng nhập ngay khi render phía server. Auth.js v5 vẫn là bản beta nên cần thử (spike 1.05) trước khi chốt.

## Quyết định

**Phương án C — BFF với Auth.js v5 (`next-auth@5.0.0-beta.32`)**:

- Phiên Auth.js (JWT mã hóa trong cookie HttpOnly) giữ access token (15 phút), refresh token (7 ngày) và thông tin người dùng của backend.
- **Credentials:** `authorize()` gọi `POST /api/v1/auth/login`; lỗi backend được trả về form qua `signIn(...).code` (ví dụ `AUTH_ACCOUNT_LOCKED`).
- **Đăng ký:** gọi `POST /api/v1/auth/register { fullName, email, userName, password }` rồi `signIn("credentials")`. `userName` do người dùng nhập (Identity `UserName`), sinh tự động khi tài khoản tạo qua Google.
- **Google:** Auth.js lấy `id_token`, server Next.js gọi `POST /api/v1/auth/google { idToken }`; backend tự xác minh bằng `Google.Apis.Auth` (audience = Client ID) và chỉ liên kết với tài khoản có sẵn khi `email_verified`. `ClientSecret` chỉ nằm ở Next.js.
- **Làm mới token:** trong callback `jwt`, khi access token còn dưới 1 phút. `proxy.ts` (Next 16) chạy trên mọi trang để Auth.js ghi lại cookie sau khi xoay vòng; `SessionProvider` hỏi lại phiên mỗi 4 phút. Refresh lỗi → `session.error` → client đăng xuất và về `/auth/login?reason=expired`.
- **Đăng xuất:** `signOut()` → sự kiện `signOut` gọi `POST /api/v1/auth/logout { refreshToken }` (endpoint `AllowAnonymous`, luôn 204).
- **Đăng nhập local ở backend:** `SignInManager.CheckPasswordSignInAsync(lockoutOnFailure: true)`, khóa 15 phút sau 5 lần sai (423); `IsActive = false` → 403 `AUTH_ACCOUNT_DISABLED`, chỉ báo sau khi mật khẩu đúng.
- **Access token phía trình duyệt:** tạm đưa vào session để `apiClient` gắn header (rủi ro chấp nhận, token chỉ sống 15 phút).

**Kết quả spike (17/09):** chạy thật backend + Next.js 16, access token 1 phút để lần nào cũng phải làm mới. Gọi phiên và trang `/profile` xen kẽ, cách nhau 35 giây (dài hơn thời gian ân hạn 30 giây của S-06) trong 140 giây: phiên luôn hợp lệ, trang trả 200, backend **không** ghi nhận lần dùng lại nào → cookie được ghi lại đúng sau mỗi lần xoay vòng. Đăng xuất thu hồi được token ở backend. → **Giữ phương án C.**

## Hệ quả

- Không có token trong `localStorage`; Server Component và proxy biết được phiên để chặn trang cần đăng nhập.
- Backend không phụ thuộc Auth.js: đổi phía frontend không ảnh hưởng API.
- Một request có thể làm mới hai lần (proxy và Server Component cùng đọc cookie cũ); lần thứ hai đi nhánh ân hạn của S-06 nên không bị đăng xuất, chỉ sinh thêm refresh token dư (tự hết hạn, job 1.20 dọn).
- Rewrite `/api/*` sang backend phải để ở nhóm `fallback` của `next.config.ts`, nếu không sẽ chặn mất `/api/auth/*` của Auth.js.
- Rủi ro chấp nhận: access token xuống trình duyệt; phản hồi 409 (email hoặc `userName` trùng) và 423 cho phép dò tài khoản — giảm bằng rate limit 5 lần/phút (S-15).

## Phương án dự phòng

Nếu Auth.js v5 gây lỗi không gỡ được (đăng xuất ngẫu nhiên, bản beta vỡ khi nâng cấp): chuyển sang **phương án B** — backend đặt refresh token trong cookie `HttpOnly; Secure; SameSite=Lax; Path=/api/v1/auth`, access token giữ trong bộ nhớ trình duyệt. API backend giữ nguyên. Hướng cải tiến sau 01/11: gọi API qua Route Handler proxy để access token không xuống trình duyệt.
