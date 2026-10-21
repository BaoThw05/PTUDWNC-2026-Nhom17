# Bàn giao TV3 cho SRS v1.1 (mục 3.22)

Người nhận: TV1, chủ sở hữu `docs/SRS_v1.1.md`. Phạm vi: 3.2, 3.5, FR-JOB-002, 5.3, 6.5, 7.5, 7.6, 8.2, 8.4 và mã lỗi Category/File/Image. Đối chiếu với mã trên `upstream/main` (`089a11c`) ngày 21/10/2026 và OpenAPI của bản demo tại `/openapi/v1.json`. Tài liệu này là nội dung để TV1 cập nhật SRS, không thay thế bản SRS chính.

## 3.2 — Danh mục (FR-CAT-001–005)

- GET `/api/v1/categories` công khai, trả mảng `{ id, name, slug, description, imageUrl, orderIndex, recipeCount }`. `recipeCount` chỉ đếm công thức Published. Thứ tự là `orderIndex` tăng dần, sau đó `name` tăng dần. Bỏ mô tả `IMemoryCache`/TTL 60 phút và sắp xếp theo tên thuần túy; phần hiện tại chỉ có hook `ICacheInvalidator` khi ghi, chưa có cache đọc. Nguồn: [`GetCategories.cs`](../../src/Backend/CulinaryBlog.Application/Features/Categories/GetCategories.cs), [`NoOpCacheInvalidator.cs`](../../src/Backend/CulinaryBlog.Infrastructure/Observability/NoOpCacheInvalidator.cs).
- GET `/api/v1/categories/{slug}` công khai, `page` mặc định 1, `pageSize` mặc định 12 và tối đa 50. Guest thấy Published; Author đăng nhập còn thấy Draft do mình viết. Slug không tồn tại trả 404 `CATEGORY_NOT_FOUND`. Không có query `sortBy` ở route này. Nguồn: [`CategoryReadEndpoints.cs`](../../src/Backend/CulinaryBlog.API/Endpoints/Categories/CategoryReadEndpoints.cs), [`GetCategoryBySlug.cs`](../../src/Backend/CulinaryBlog.Application/Features/Categories/GetCategoryBySlug.cs), [`PagedResult.cs`](../../src/Backend/CulinaryBlog.Application/Common/Models/PagedResult.cs).
- POST `/api/v1/categories` yêu cầu Admin, `name` 2–100 ký tự, không chứa HTML; `description` tối đa 2000, `imageUrl` tối đa 500, `orderIndex` không âm. Tên duy nhất không phân biệt hoa/thường; slug tự sinh với hậu tố khi trùng. Trả 201 và `Location` theo slug. PUT `/{id:guid}` yêu cầu Admin, trả 200; đổi tên không đổi slug. Nguồn: [`CreateCategory.cs`](../../src/Backend/CulinaryBlog.Application/Features/Categories/CreateCategory.cs), [`UpdateCategory.cs`](../../src/Backend/CulinaryBlog.Application/Features/Categories/UpdateCategory.cs), [`CategoriesEndpoints.cs`](../../src/Backend/CulinaryBlog.API/Endpoints/Categories/CategoriesEndpoints.cs).
- DELETE `/{id:guid}` yêu cầu Admin, **xóa vật lý** và trả 204. Nếu còn công thức tham chiếu (kể cả công thức đã xóa mềm), trả 409 `CATEGORY_DELETE_HAS_RECIPES`; ID không tồn tại trả 404. Sửa các chỗ gọi đây là soft delete. Nguồn: [`DeleteCategory.cs`](../../src/Backend/CulinaryBlog.Application/Features/Categories/DeleteCategory.cs).

## 3.5 và FR-RCP-008 — File/ảnh công thức

- Abstraction thực tế là `IFileStorage` với `UploadAsync(Stream, key, contentType)`, `OpenReadAsync`, `DeleteAsync(key)`, `DeleteByPrefixAsync(prefix)`, `GetPublicUrl(key)`. DB lưu **object key**, URL công khai được dựng khi trả DTO. Storage cấu hình được giữa S3 và Local. Sửa `IFileStorageService`, API dựa trên `IFormFile`, và mô tả lưu URL vào DB. Nguồn: [`IFileStorage.cs`](../../src/Backend/CulinaryBlog.Application/Abstractions/IFileStorage.cs), [`S3FileStorage.cs`](../../src/Backend/CulinaryBlog.Infrastructure/Storage/S3FileStorage.cs).
- Upload `POST /api/v1/recipes/{recipeId}/images` dùng multipart `file`, `altText?`. Tối đa **5 MiB**; chỉ JPEG, PNG, WebP, kiểm tra MIME khớp chữ ký; AVIF chưa được hỗ trợ. Key ảnh gốc: `recipes/{recipeId}/{imageId}/original.{ext}`. Ảnh đầu tiên thành primary. Trả 201 với `{ imageId, originalUrl, mediumUrl, thumbnailUrl, altText, isPrimary, orderIndex }`. Quá cỡ trả **413** `FILE_SIZE_EXCEEDED`; loại/chữ ký sai trả **415** `FILE_TYPE_NOT_ALLOWED`; storage lỗi trả **503** `STORAGE_UNAVAILABLE`. Nguồn: [`UploadRecipeImage.cs`](../../src/Backend/CulinaryBlog.Application/Features/RecipeImages/UploadRecipeImage.cs), [`RecipeImagesEndpoints.cs`](../../src/Backend/CulinaryBlog.API/Endpoints/RecipeImages/RecipeImagesEndpoints.cs).
- Có `GET /api/v1/recipes/{recipeId}/images` trả danh sách. Chọn ảnh chính và sửa `altText`/`orderIndex` qua `PATCH /api/v1/recipes/{recipeId}/images/{imageId}` với body `{ altText?, isPrimary?, orderIndex? }`; **không có** route `/{imageId}/primary`. `altText` tối đa 500 ký tự. DELETE cùng đường dẫn ảnh trả 204, xóa DB rồi enqueue xóa cả prefix; nếu xóa ảnh primary, chọn ảnh còn lại có `orderIndex` thấp nhất (sau đó `id`). Cả bốn route yêu cầu Author/Admin và kiểm tra quyền sở hữu. Nguồn: [`RecipeImagesEndpoints.cs`](../../src/Backend/CulinaryBlog.API/Endpoints/RecipeImages/RecipeImagesEndpoints.cs), [`UpdateRecipeImage.cs`](../../src/Backend/CulinaryBlog.Application/Features/RecipeImages/UpdateRecipeImage.cs), [`DeleteRecipeImage.cs`](../../src/Backend/CulinaryBlog.Application/Features/RecipeImages/DeleteRecipeImage.cs).

## FR-JOB-002 — Resize

Hangfire enqueue sau khi upload. SkiaSharp tạo `thumb.webp` **300×300 crop giữa** và `medium.webp` **rộng tối đa 800 px, giữ tỉ lệ**, không phóng lớn ảnh nhỏ; chất lượng WebP 85. Lưu `ThumbnailKey`/`MediumKey`, không lưu URL biến thể. Decode lỗi được ghi `IsProcessingFailed`/`ProcessingError` rồi kết thúc job, tránh retry vô hạn; lỗi hạ tầng vẫn theo retry toàn cục 3 lần (60/300/1800 giây). Sửa mô tả medium cố định 800×600 và retry mọi lỗi. Nguồn: [`RecipeImageResizeJob.cs`](../../src/Backend/CulinaryBlog.Infrastructure/RecipeImages/RecipeImageResizeJob.cs), [`DependencyInjection.cs`](../../src/Backend/CulinaryBlog.Infrastructure/DependencyInjection.cs).

## 5.3 và 6.5 — Dịch vụ, triển khai

- MinIO dùng `AWSSDK.S3`, cấu hình `FileStorage:Provider`, `FileStorage:PublicBaseUrl`, `FileStorage:S3:ServiceUrl/AccessKey/SecretKey/BucketName` (biến môi trường dạng `FileStorage__...`); trình duyệt đọc ảnh qua `/media/` của Nginx. Không có direct browser upload/presigned URL. Nguồn: [`DependencyInjection.cs`](../../src/Backend/CulinaryBlog.Infrastructure/DependencyInjection.cs), [`nginx.conf.template`](../../docker/nginx/nginx.conf.template).
- Hangfire server chạy trong API với PostgreSQL; dashboard trong API chỉ bật ở Development. Nginx xuất `/hangfire` với **Basic Auth** (không dùng chính sách JWT Admin). Cloudflare Tunnel là profile `demo` của Compose; token `CLOUDFLARE_TUNNEL_TOKEN` chỉ đặt ở `.env` local/secret store, có thể dùng quick tunnel khi chưa có domain. Nguồn: [`Program.cs`](../../src/Backend/CulinaryBlog.API/Program.cs), [`docker-compose.yml`](../../docker-compose.yml), [`cloudflare-tunnel.md`](../research/cloudflare-tunnel.md).
- Compose thực tế dùng Nginx làm điểm vào duy nhất qua `${APP_PORT}:80`; API/web/PostgreSQL/Redis/MinIO không publish cổng host. Có PostgreSQL 16, Redis có mật khẩu, MinIO ghim digest và job tạo bucket, Seq ghim phiên bản, **Mailpit** thay Mailhog, `cloudflared` profile demo. Không có `docker-compose.prod.yml` trong repo. Không chép lại bảng image/port cũ ở 6.5. Nguồn: [`docker-compose.yml`](../../docker-compose.yml), [`.env.example`](../../.env.example).

## 7.5 và 7.6 — Mô hình dữ liệu

- `RecipeImage`: `OriginalKey` bắt buộc, `MediumKey`/`ThumbnailKey` nullable (tối đa 500); thêm `IsProcessingFailed` và `ProcessingError` (tối đa 500); `AltText` tối đa **500**, `IsPrimary`, `OrderIndex`. Có unique partial index đảm bảo tối đa một ảnh primary cho mỗi recipe. Thay ba cột `*Url` trong SRS bằng `*Key`. Nguồn: [`RecipeImage.cs`](../../src/Backend/CulinaryBlog.Domain/Entities/RecipeImage.cs), [`RecipeImageConfiguration.cs`](../../src/Backend/CulinaryBlog.Infrastructure/RecipeImages/RecipeImageConfiguration.cs).
- `Category`: `Name` là PostgreSQL `citext` tối đa 100 và unique không phân biệt hoa/thường; `Slug` tối đa 120 và unique, `Description` text, `ImageUrl` tối đa 500, `OrderIndex` mặc định 0. Nguồn: [`CategoryConfiguration.cs`](../../src/Backend/CulinaryBlog.Infrastructure/Categories/CategoryConfiguration.cs).

## 8.2, 8.4 và mã lỗi

Thay bảng endpoint ở 8.2/8.4 theo các route, request và response phía trên. Cụ thể thêm GET danh sách ảnh, xóa `isPrimary` khỏi form upload, xóa `sortBy` khỏi chi tiết category, sửa pageSize mặc định 12, DELETE category là xóa vật lý và dùng mã 413/415 cho upload lỗi.

Problem Details theo **RFC 9457**, mã ứng dụng ở trường mở rộng `code` (không phải `type`). Bảng mã TV3 cần có:

| Mã | HTTP | Ý nghĩa |
|---|---:|---|
| `CATEGORY_NOT_FOUND` | 404 | Không tìm thấy danh mục |
| `CATEGORY_NAME_EXISTS` | 409 | Tên danh mục đã tồn tại |
| `CATEGORY_DELETE_HAS_RECIPES` | 409 | Danh mục còn công thức |
| `IMAGE_NOT_FOUND` | 404 | Không tìm thấy ảnh thuộc công thức |
| `IMAGE_ALT_TEXT_INVALID` | 422 | Alt text quá 500 ký tự |
| `IMAGE_ORDER_INVALID` | 422 | `orderIndex` âm |
| `IMAGE_PRIMARY_REQUIRED` | 422 | Bỏ ảnh primary mà chưa chọn ảnh thay thế |
| `FILE_SIZE_EXCEEDED` | 413 | File quá 5 MiB |
| `FILE_TYPE_NOT_ALLOWED` | 415 | MIME/chữ ký không hợp lệ hoặc không khớp |
| `STORAGE_UNAVAILABLE` | 503 | Không thể lưu ảnh |

Nguồn: [`CategoryAdminDto.cs`](../../src/Backend/CulinaryBlog.Application/Features/Categories/CategoryAdminDto.cs), [`GlobalExceptionHandler.cs`](../../src/Backend/CulinaryBlog.API/ErrorHandling/GlobalExceptionHandler.cs), [`error-codes.md`](../api/error-codes.md). `docs/api/error-codes.md` hiện **thiếu ba dòng CATEGORY** (hàng trống ở mục TV3); TV2/TV3 cần điền bằng PR riêng vì đây là file dùng chung theo `docs/OWNERSHIP.md`.

### Lưu ý OpenAPI hiện tại

OpenAPI live xác nhận các đường dẫn và method ở trên: 5 path, 9 operation cho Category/RecipeImages. Tuy nhiên bản sinh tự động hiện chỉ liệt kê response `200` cho mọi operation, kể cả POST thực trả 201 và DELETE thực trả 204; chưa thể dùng phần response OpenAPI làm nguồn chuẩn cho SRS. Status trong bản bàn giao lấy từ implementation và tài liệu mã lỗi. Cần bổ sung metadata `Produces`/typed result cho endpoint trong một việc riêng rồi đối chiếu lại OpenAPI. Nguồn: [`CategoriesEndpoints.cs`](../../src/Backend/CulinaryBlog.API/Endpoints/Categories/CategoriesEndpoints.cs), [`RecipeImagesEndpoints.cs`](../../src/Backend/CulinaryBlog.API/Endpoints/RecipeImages/RecipeImagesEndpoints.cs).
