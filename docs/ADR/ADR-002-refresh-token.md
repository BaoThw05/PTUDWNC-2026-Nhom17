# ADR-002 · S-06 · Refresh token: xoay vòng và phát hiện dùng lại

- **Trạng thái:** Đã chốt
- **Ngày:** 17/09/2026
- **Người viết:** TV1
- **Liên quan:** D-07, G-05 · FR-AUTH-001, FR-AUTH-004, NFR-SEC-002, 7.8 · CR-05

## Bối cảnh

FR-AUTH-004 và NFR-SEC-002 bắt buộc xoay vòng refresh token và phát hiện token cũ bị dùng lại. Nếu thu hồi cả chuỗi ngay khi thấy token cũ, người dùng mở nhiều tab (hoặc Auth.js làm mới hai lần trong một request, xem ADR 0001) sẽ bị đăng xuất nhầm. SRS cũng ghi hai độ dài token khác nhau (128-bit và 512-bit).

## Quyết định

**Phương án B — xoay vòng có thời gian ân hạn 30 giây:**

- Token: 32 byte ngẫu nhiên (`RandomNumberGenerator`), mã hóa base64url; DB **chỉ lưu SHA-256** dạng hex 64 ký tự. Hạn 7 ngày.
- Bảng `RefreshTokens`: `Id`, `UserId`, `FamilyId`, `TokenHash` (unique), `ExpiresAt`, `RevokedAt`, `RevokedReason` (`Rotated` / `Logout` / `ReuseDetected`), `ReplacedByTokenHash`, `CreatedByIp`, `CreatedAt`, `UpdatedAt`; kiểm soát đồng thời bằng `xmin`. Bỏ cột `IsRevoked` (dùng `RevokedAt IS NOT NULL`).
- Mỗi lần đăng nhập mở một `FamilyId` mới.
- `POST /auth/refresh`:
  - Token còn hiệu lực → thu hồi (`Rotated`), cấp token mới cùng family.
  - Token `Rotated` được dùng lại **trong 30 giây** → cấp token mới, không thu hồi gì thêm.
  - Token `Rotated` được dùng lại **sau 30 giây** → thu hồi mọi token còn hiệu lực của family (`ReuseDetected`), ghi log cảnh báo bảo mật, trả 401 `AUTH_REFRESH_TOKEN_REVOKED`.
  - Token `Logout`/`ReuseDetected` → 401, không thu hồi family.
  - Hết hạn → 401 `AUTH_REFRESH_TOKEN_EXPIRED`; không tồn tại → 401 `AUTH_REFRESH_TOKEN_INVALID`; tài khoản bị vô hiệu hóa → 403.
  - Hai request cùng xoay vòng một token: request thua gặp xung đột `xmin`, đọc lại token (lúc này đã `Rotated`) và đi nhánh ân hạn.
- Hangfire recurring job xóa token đã hết hạn quá 30 ngày (việc 1.20, chờ Hangfire của TV3).

## Hệ quả

- Lộ token vẫn bị phát hiện nếu kẻ tấn công dùng sau 30 giây; nhiều tab không bị đăng xuất nhầm.
- Trong 30 giây đó, token cũ đổi được thêm token mới — rủi ro chấp nhận, cửa sổ ngắn.
- Mỗi lần ân hạn sinh thêm một token trong family; bảng lớn dần cho tới khi job dọn dẹp chạy.
- Đã có unit test và integration test (PostgreSQL thật) cho: xoay vòng, dùng lại trong/sau ân hạn, hết hạn, sau đăng xuất, xung đột đồng thời.

## Phương án dự phòng

Nếu log "reuse detected" xuất hiện khi người dùng thao tác bình thường: kiểm tra lại thời gian ân hạn và thời điểm làm mới phía Auth.js (hiện làm mới trước 1 phút). Nếu cần an toàn hơn, giảm ân hạn xuống 10 giây và khóa (serialize) việc làm mới ở phía Next.js.
