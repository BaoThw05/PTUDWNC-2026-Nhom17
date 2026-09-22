# Bảng mã lỗi API

Mọi lỗi trả về **Problem Details (RFC 9457)** với `Content-Type: application/problem+json` và hai trường mở rộng:

- `code` — mã lỗi `SCREAMING_SNAKE_CASE`, có tiền tố module. Frontend hiển thị thông báo theo `code`, không theo `detail`.
- `traceId` — mã truy vết để tìm log.
- `errors` — chỉ có với lỗi validation: `{ "tênTrường": ["thông báo", …] }`.

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.21",
  "title": "Unprocessable Entity",
  "status": 422,
  "detail": "One or more validation errors occurred.",
  "code": "VALIDATION_ERROR",
  "traceId": "00-…",
  "errors": { "Title": ["'Title' must not be empty."] }
}
```

**Quy tắc:** mã lỗi mới phải được thêm vào file này **trong cùng PR** với code sinh ra nó. Hằng số dùng chung nằm ở `src/Backend/CulinaryBlog.Application/Common/Errors/ErrorCodes.cs`; mã riêng đặt trong thư mục module.

## Mã chung

| Mã | HTTP | Mô tả | Module |
|---|---|---|---|
| `BAD_REQUEST` | 400 | Body hoặc tham số sai định dạng (JSON hỏng, kiểu sai) | Chung |
| `UNAUTHORIZED` | 401 | Thiếu hoặc sai access token | Chung |
| `FORBIDDEN` | 403 | Đã xác thực nhưng không đủ quyền | Chung |
| `NOT_FOUND` | 404 | Không tìm thấy tài nguyên hoặc route | Chung |
| `CONFLICT` | 409 | Trùng dữ liệu duy nhất hoặc xung đột trạng thái | Chung |
| `CONCURRENCY_CONFLICT` | 409 | `version` gửi lên không khớp (S-04) | Chung |
| `VALIDATION_ERROR` | 422 | Dữ liệu không hợp lệ (FluentValidation) | Chung |
| `TOO_MANY_REQUESTS` | 429 | Vượt giới hạn request (kèm `Retry-After`) | Chung |
| `INTERNAL_ERROR` | 500 | Lỗi không lường trước; không trả `detail` | Chung |

## Auth — TV1

| Mã | HTTP | Mô tả | Module |
|---|---|---|---|
| `AUTH_EMAIL_EXISTS` | 409 | Email đã được đăng ký (so sánh không phân biệt hoa/thường) | Auth |
| `AUTH_USERNAME_EXISTS` | 409 | Tên đăng nhập (`userName`) đã được dùng (so sánh không phân biệt hoa/thường) | Auth |
| `AUTH_INVALID_CREDENTIALS` | 401 | Email không tồn tại hoặc sai mật khẩu — dùng chung một mã để không lộ email đã đăng ký | Auth |
| `AUTH_ACCOUNT_LOCKED` | 423 | Tài khoản bị khóa 15 phút sau 5 lần nhập sai mật khẩu | Auth |
| `AUTH_ACCOUNT_DISABLED` | 403 | Tài khoản bị vô hiệu hóa (`IsActive = false`); chỉ báo khi mật khẩu đúng | Auth |
| `AUTH_REFRESH_TOKEN_INVALID` | 401 | Refresh token không tồn tại | Auth |
| `AUTH_REFRESH_TOKEN_EXPIRED` | 401 | Refresh token đã hết hạn (7 ngày) | Auth |
| `AUTH_REFRESH_TOKEN_REVOKED` | 401 | Refresh token đã bị thu hồi (đăng xuất, hoặc bị dùng lại sau 30 giây → thu hồi cả family) | Auth |
| `AUTH_USER_NOT_FOUND` | 404 | Người dùng trong access token không còn tồn tại | Auth |
| `AUTH_GOOGLE_TOKEN_INVALID` | 401 | `idToken` Google sai chữ ký, sai audience hoặc hết hạn | Auth |
| `AUTH_GOOGLE_EMAIL_UNVERIFIED` | 401 | Email của tài khoản Google chưa được Google xác minh | Auth |
| `AUTH_GOOGLE_UNAVAILABLE` | 502 | Không xác minh được với Google, hoặc backend chưa cấu hình Google Client ID | Auth |

## Recipes — TV2

| Mã | HTTP | Mô tả | Module |
|---|---|---|---|
| | | | Recipes |

## Categories / RecipeImages / File — TV3

| Mã | HTTP | Mô tả | Module |
|---|---|---|---|
| | | | Categories |
| | | | RecipeImages |

## Search — TV4

| Mã | HTTP | Mô tả | Module |
|---|---|---|---|
| | | | RecipeSearch |
