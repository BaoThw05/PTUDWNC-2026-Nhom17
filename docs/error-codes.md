# Bảng mã lỗi chung — Culinary Blog API

> Mọi lỗi trả về theo chuẩn Problem Details (RFC 9457), có thêm trường `code` để client phân biệt loại lỗi
> cụ thể (vì nhiều lỗi khác nhau có thể cùng dùng 1 mã HTTP, ví dụ 404 hay 422).
>
> Format response mẫu:
> ```json
> {
>   "type": "https://culinaryblog.dev/errors/recipe-not-found",
>   "title": "Recipe not found",
>   "status": 404,
>   "code": "RECIPE_NOT_FOUND",
>   "traceId": "00-abc123...",
>   "errors": { }
> }
> ```

## Quy ước đặt mã lỗi
- Format: `<MODULE>_<MÔ_TẢ_NGẮN>`, viết hoa, cách nhau bằng gạch dưới.
- `MODULE` là tiền tố module: `RECIPE`, `AUTH`, `CATEGORY`, `FILE`, `SEARCH`.
- Mỗi thành viên tự thêm dòng của module mình vào bảng dưới, không sửa dòng của module khác.

## Bảng mã lỗi

| Mã | HTTP | Mô tả | Module |
|---|---|---|---|
| `VALIDATION_ERROR` | 422 | Dữ liệu đầu vào không hợp lệ (FluentValidation) | Chung |
| `UNAUTHORIZED` | 401 | Chưa đăng nhập hoặc token không hợp lệ | Chung |
| `FORBIDDEN` | 403 | Đã đăng nhập nhưng không có quyền thực hiện | Chung |
| `INTERNAL_SERVER_ERROR` | 500 | Lỗi hệ thống không mong muốn | Chung |
| `RECIPE_NOT_FOUND` | 404 | Không tìm thấy Recipe (hoặc không thuộc quyền xem) | Recipe |
| `RECIPE_FORBIDDEN` | 403 | Không phải chủ sở hữu Recipe (và không phải Admin) | Recipe |
| `RECIPE_CONCURRENCY_CONFLICT` | 409 | `version` gửi lên không khớp dữ liệu hiện tại | Recipe |
| `RECIPE_PUBLISH_INCOMPLETE` | 422 | Chưa đủ điều kiện publish (chưa có bước nào) | Recipe |
| `RECIPE_CATEGORY_INVALID` | 422 | `CategoryId` gửi lên không tồn tại | Recipe |
| `RECIPE_INVALID_STATE_TRANSITION` | 422 | Chuyển trạng thái không hợp lệ (ví dụ Archived → Published) | Recipe |
| `STEP_NOT_FOUND` | 404 | Không tìm thấy RecipeStep | Recipe |
| `INGREDIENT_NOT_FOUND` | 404 | Không tìm thấy RecipeIngredient | Recipe |

*(TV1, TV3, TV4: thêm mã lỗi module của bạn vào cuối bảng trên theo đúng format)*

## Quy ước đặt tên chung (Naming Convention)

| Loại | Quy ước | Ví dụ |
|---|---|---|
| Route API | số nhiều, kebab-case | `/api/v1/recipes`, `/api/v1/recipe-ingredients` |
| Command (ghi) | `<Verb><Entity>Command` | `CreateRecipeCommand`, `PublishRecipeCommand` |
| Query (đọc) | `Get<Entity>By<Field>Query` | `GetRecipeBySlugQuery`, `GetRecipesQuery` |
| Handler | `<Command/Query name>Handler` | `CreateRecipeCommandHandler` |
| DTO trả về | `<Entity>Dto`, `<Entity>DetailDto` | `RecipeDto`, `RecipeDetailDto` |
| JSON property | camelCase, trùng tên thuộc tính C# | `createdAt`, `prepTimeMinutes` |
| Sắp xếp (`sort`) | tiền tố `-` cho giảm dần | `sort=-createdAt` (mới nhất trước) |
| Phân trang | `page` (bắt đầu từ 1), `pageSize` | `?page=1&pageSize=20` |