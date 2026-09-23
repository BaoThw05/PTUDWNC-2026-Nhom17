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
| | | | Auth |

## Recipes — TV2

| Mã | HTTP | Mô tả | Module |
|---|---|---|---|
| `RECIPE_NOT_FOUND` | 404 | Không tìm thấy Recipe (hoặc không thuộc quyền xem) | Recipes |
| `RECIPE_FORBIDDEN` | 403 | Không phải tác giả của công thức (và không phải Admin) | Recipes |
| `RECIPE_CONCURRENCY_CONFLICT` | 409 | Phiên bản `version` gửi lên không khớp với dữ liệu hiện tại (S-04) | Recipes |
| `RECIPE_PUBLISH_INCOMPLETE` | 422 | Chưa đủ điều kiện xuất bản (chưa có bước nào hoặc xóa bước cuối khi đang Published) | Recipes |
| `RECIPE_CATEGORY_INVALID` | 422 | `CategoryId` gửi lên không tồn tại hoặc không hợp lệ | Recipes |
| `RECIPE_INVALID_STATE_TRANSITION` | 422 | Chuyển đổi trạng thái công thức không hợp lệ | Recipes |
| `STEP_NOT_FOUND` | 404 | Không tìm thấy bước thực hiện (RecipeStep) | Recipes |
| `INGREDIENT_NOT_FOUND` | 404 | Không tìm thấy nguyên liệu (RecipeIngredient) | Recipes |

## Categories / RecipeImages / File — TV3

| Mã | HTTP | Mô tả | Module |
|---|---|---|---|
| | | | Categories |
| | | | RecipeImages |

## Search — TV4

| Mã | HTTP | Mô tả | Module |
|---|---|---|---|
| | | | RecipeSearch |
