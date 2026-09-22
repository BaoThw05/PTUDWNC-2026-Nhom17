# PHÂN TÍCH GIẢI PHÁP & HƯỚNG ĐI KỸ THUẬT — CULINARY BLOG v1.1.0

**Tài liệu tham chiếu:** SRS_Culinary_Blog_v1.0.0_DanhSachLoi.md  
**Mục tiêu:** Đưa ra phương án xử lý dứt điểm cho **74 vấn đề kỹ thuật (Nhóm A — F)**, thiết lập kiến trúc chuẩn cho .NET 10 Minimal API, Next.js App Router, PostgreSQL 16 và S3 Storage.

---

## QUY TẮC PHÂN CẤP QUYẾT ĐỊNH (ARCHITECTURE DECISION RULES)

1. **An toàn & Bảo mật là ưu tiên hàng đầu:** Mọi thiết kế phải triệt tiêu lỗ hổng bảo mật (Authentication, Authorization, Mass Assignment, XSS, SQL Injection).
2. **Clean Architecture 4 tầng chuẩn mực:** `Domain` giữ 0 NuGet dependencies; `Application` chứa toàn bộ Interfaces, CQRS Handlers, Validators; `Infrastructure` chứa DbContext, External Services; `API` chứa Minimal Endpoints và Middlewares.
3. **Thực dụng & Kiểm chứng được:** Lựa chọn giải pháp phù hợp với năng lực nhóm 4 sinh viên, hoàn thành đúng tiến độ 6 tuần.

---

## PHẦN 1: CÁC QUYẾT ĐỊNH CHẶN (BLOCKING DECISIONS - S-03 ĐẾN S-10)

### S-03 · Xóa dữ liệu: Hard Delete hay Soft Delete? (Giải quyết A-01, A-07, C-21)
* **Phân tích:** Nếu trộn lẫn Soft Delete cho tất cả Entity sẽ làm hỏng `ON DELETE CASCADE`, vi phạm UNIQUE constraint trên `Slug` và `Category.Name`, đồng thời khiến logic tính `recipeCount` bị sai.
* **QUYẾT ĐỊNH CHỌN (Phương án C - Mô hình Lai):**
  * **`Recipe` (Aggregate Root):** Áp dụng **Soft Delete** (`IsDeleted`, `DeletedAt`). Bài viết bị xóa sẽ chuyển vào "Thùng rác" và được dọn dẹp vĩnh viễn sau 30 ngày bởi Hangfire Recurring Job. Chuyển `Recipe.Slug` thành Partial Unique Index: `CREATE UNIQUE INDEX "IX_Recipes_Slug" ON "Recipes"("Slug") WHERE "IsDeleted" = false;`.
  * **Các Entity con (`RecipeStep`, `RecipeIngredient`, `RecipeImage`) & `Category`:** Áp dụng **Hard Delete**. Xóa bài viết khỏi DB sẽ trigger CASCADE delete xóa sạch bảng con. Xóa danh mục phải kiểm tra danh mục không còn chứa bài viết (báo `409 Conflict` nếu còn).

---

### S-04 · Kiểm soát Cập nhật Đồng thời - Optimistic Concurrency Control (Giải quyết C-01, C-28, B-07)
* **Phân tích:** PostgreSQL/Npgsql không hỗ trợ `[Timestamp]` / `bytea` tự tăng của SQL Server.
* **QUYẾT ĐỊNH CHỌN (Npgsql `xmin` + Version field):**
  * Áp dụng cột hệ thống `xmin` của PostgreSQL cho `Recipe`:
    ```csharp
    builder.Entity<Recipe>()
           .Property<uint>("Version")
           .HasFieldName("xmin")
           .HasColumnType("xid")
           .ValueGeneratedOnAddOrEdit()
           .IsRowVersion();
    ```
  * Client gửi `version` trong Body của request `PUT /api/v1/recipes/{id}`. Nếu lệch `version` -> trả về **`409 Conflict`** (`RECIPE_CONCURRENCY_CONFLICT`).
  * Khi cập nhật bảng con (`Steps`, `Ingredients`, `Images`), Handler phải chủ động cập nhật `Recipe.UpdatedAt = DateTime.UtcNow` trong cùng Transaction để buộc PostgreSQL tăng `xmin` của `Recipe`.

---

### S-05 · Kiến trúc Xác thực Auth.js v5 ↔ .NET BFF & Google OAuth (Giải quyết B-01, B-02, B-03, C-05, C-06, C-08, C-09, F-08)
* **Phân tích:** Tránh việc Backend nhận thông tin Google giả mạo từ Frontend; thống nhất hợp đồng API Auth.
* **QUYẾT ĐỊNH CHỌN (BFF Pattern + Validated Google ID Token):**
  1. **Google Auth (`POST /api/v1/auth/google`):** Frontend lấy `idToken` từ Google Identity Services và gửi `{ idToken }` về Backend. Backend dùng thư viện `GoogleJsonWebSignature.ValidateAsync()` để xác thực chữ ký, Audience, Expiry và trạng thái `email_verified`.
  2. **Tự động tạo / Liên kết tài khoản:** Nếu email từ Google đã xác thực và chưa có trong DB -> Tự động tạo `ApplicationUser` với Role `Author`, sinh `UserName` tự động. Nếu email đã tồn tại -> Tự động liên kết Google Login.
  3. **Hợp đồng Đăng ký (`POST /api/v1/auth/register`):** Body nhận `{ fullName, email, userName, password }`. Trả về `201 Created` kèm `AuthResponseDto` (`accessToken`, `refreshToken`, `expiresAt`, `user`) để tự động đăng nhập.
  4. **Lưu trữ Token:** Refresh Token (hash SHA-256) lưu tại Database PostgreSQL (`RefreshTokens` table). Access Token (15 phút) lưu tại Memory ở Frontend.

---

### S-06 · Refresh Token Rotation & Chống Race Condition (Giải quyết C-07)
* **Phân tích:** Mở nhiều tab đồng thời gửi request refresh token dễ gây thu hồi nhầm tài khoản do bị tính là Reuse Attack.
* **QUYẾT ĐỊNH CHỌN (Rotation + FamilyId + Ân hạn 30 giây):**
  * MỗiRefresh Token có `FamilyId` (Guid).
  * Khi Refresh: Vô hiệu hóa token cũ (`IsRevoked = true`, `ReplacedByToken = newToken`), cấp cặp Token mới.
  * **Thời gian ân hạn 30 giây (Grace Period):** Nếu một Refresh Token vừa bị thay thế trong vòng 30 giây được gửi lại (do race condition giữa các tab) -> Hệ thống trả về cặp Token mới nhất đã tạo trước đó thay vì thu hồi toàn bộ Family. Nếu token đã bị thay thế quá 30 giây bị gửi lại -> Coi là Reuse Attack -> Revoke toàn bộ Token thuộc `FamilyId` đó và ném lỗi `401`.

---

### S-07 · Chiến lược Caching Nâng cao (Giải quyết C-03, C-04)
* **Phân tích:** Khắc phục lỗi Output Cache tĩnh bị lộ bài nháp (Draft) cho Guest và TTL mâu thuẫn.
* **QUYẾT ĐỊNH CHỌN (ASP.NET Core Output Caching + Redis Store + Eviction Tags):**
  * **Tách biệt Endpoints Công khai & Cá nhân:**
    * Endpoints công khai (`GET /api/v1/recipes`, `GET /api/v1/recipes/{slug}`, `GET /api/v1/categories`) **chỉ trả dữ liệu `Published`**. Các endpoints này được bật Output Cache lưu tại Redis với TTL = 15 phút.
    * Endpoints cá nhân (`GET /api/v1/me/recipes`, `GET /api/v1/recipes/{id}`) yêu cầu Bearer Token -> **Bỏ qua Output Cache** (luôn đọc từ DB/Redis Cache-Aside).
  * **Xóa Cache theo Tag (Tag-based Invalidation):** Khi có bất kỳ mutation nào (Create/Update/Delete/Publish) -> Kích hoạt `IOutputCacheStore.EvictByTagAsync("recipes")` hoặc `EvictByTagAsync($"recipe:{slug}")`.

---

### S-08 · Full-Text Search Tiếng Việt Không Dấu (Giải quyết C-02, C-23)
* **Phân tích:** Cấu hình FTS chuẩn xác trên PostgreSQL 16.
* **QUYẾT ĐỊNH CHỌN (Custom Text Search Config + Unaccent + GIN Index):**
  * Viết Raw SQL Migration tạo extension và dictionary:
    ```sql
    CREATE EXTENSION IF NOT EXISTS unaccent;
    CREATE EXTENSION IF NOT EXISTS pg_trgm;
    ```
  * Cột `SearchVector` (`tsvector`) được tự động duy trì bằng PostgreSQL Trigger khi `Title` hoặc `Description` thay đổi.
  * Cấu hình trọng số: `Title` (Trọng số A), `Description` (Trọng số B).
  * Trình xử lý truy vấn sử dụng `EF.Functions.ToTsQuery` kết hợp `unaccent` để hỗ trợ tìm kiếm từ khóa không dấu hoặc có dấu với độ chính xác cao.

---

### S-09 · Trình Trừu tượng hóa Lưu trữ Tệp tin - S3 Abstraction (Giải quyết E-01, C-14, C-15)
* **Phân tích:** Khắc phục việc MinIO bản cộng đồng bị gỡ bỏ khỏi Docker Hub.
* **QUYẾT ĐỊNH CHỌN (AWSSDK.S3 Storage Service):**
  * Xây dựng `IFileStorageService` dựa trên chuẩn AWS S3 API.
  * Môi trường Local Dev: Chạy LocalStack hoặc MinIO bản đã kiểm định (ghim tag phiên bản cụ thể). Môi trường Demo/Production: Sử dụng Cloudflare R2 / AWS S3 / DigitalOcean Spaces.
  * **Quy tắc Lưu trữ:**
    * Validate file phía Server: Max 5MB, đọc Magic Bytes thực tế của tệp (JPEG, PNG, WebP, AVIF).
    * Lưu **Object Key** (`recipes/{recipeId}/{imageId}/original.webp`) vào CSDL thay vì lưu URL tuyệt đối. Khi trả API, tự động nối `PublicBaseUrl`.

---

### S-10 · Phản hồi REST API Chuẩn hóa & Mã Lỗi Tập trung (Giải quyết B-04, B-05, A-08, A-09)
* **QUYẾT ĐỊNH CHỌN (RFC 9457 Problem Details + Unwrapped PagedResult):**
  * **Thành công (Paginated):** Trả về trực tiếp `PagedResult<T>` không bọc vỏ thừa:
    ```json
    {
      "items": [...],
      "page": 1,
      "pageSize": 10,
      "totalCount": 100,
      "totalPages": 10,
      "hasNextPage": true,
      "hasPreviousPage": false
    }
    ```
  * **Lỗi (RFC 9457):** Sử dụng `application/problem+json`. Lỗi Validation trả về **`422 Unprocessable Entity`** kèm chi tiết từng trường. Lỗi Xung đột/Concurrency trả về **`409 Conflict`**.

---

## PHẦN 2: BẢNG GIẢI PHÁP CHI TIẾT THEO TỪNG MÃ LỖI (A-01 ĐẾN F-08)

| Mã lỗi | Hướng xử lý kỹ thuật chi tiết | Tầng thực thi |
| :--- | :--- | :--- |
| **A-01** | Áp dụng Soft Delete cho `Recipe` (chuyển thùng rác), Hard Delete cho `Category` và các bảng con. Thêm Partial Unique Index cho `Slug`. | Domain / Infra |
| **A-02** | Thống nhất điều kiện Publish: `Recipe` phải có ít nhất 1 `Step` và 1 `Ingredient`. Có thể bổ sung cờ cấu hình bắt buộc `Image`. | Application |
| **A-03** | Bổ sung endpoint `PATCH /api/v1/recipes/{id}/unarchive`. Giữ nguyên `PublishedAt` khi Unpublish để bảo toàn ngày SEO. | API / App |
| **A-04** | Tự động sinh Slug kèm hậu tố `-2`, `-3` nếu trùng tên. Khi bài viết đã Publish lần đầu, `Slug` trở thành bất biến. | Application |
| **A-05** | Báo lỗi `409 Conflict` (`CATEGORY_NOT_EMPTY`) nếu xóa danh mục đang chứa bài viết. | Application |
| **A-06** | Kiểm tra trùng tên Danh mục (Case-insensitive) -> trả `409`. Chuẩn hóa độ dài `Name` 2–100 ký tự. | Application |
| **A-07** | Cột `recipeCount` trong danh mục chỉ đếm các bài viết có trạng thái `Published` và `IsDeleted == false`. | Infrastructure |
| **A-08** | Chuẩn hóa Enum `Difficulty` (`Easy`, `Medium`, `Hard`). `Quantity`/`Unit` cho phép `null` cho nguyên liệu "vừa đủ". | Domain |
| **A-09** | Cấu hình System.Text.Json `PropertyNamingPolicy = JsonNamingPolicy.CamelCase` toàn hệ thống. | API |
| **A-10** | Định nghĩa "Recipe nổi bật" = Các bài xuất bản mới nhất có hình ảnh. Trường `Bio` bổ sung vào DTO hồ sơ cá nhân. | App / API |
| **A-11** | Loại bỏ Policy `VerifiedAuthor` mồ côi. Mọi tài khoản đăng ký tự động được gán Role `Author`. Admin được gán cả Role `Admin` và `Author`. | Application |
| **B-01** | Khôi phục luồng `POST /auth/register` chuẩn: Nhận `{fullName, email, userName, password}`, trả `AuthResponseDto` tự động đăng nhập. | API / App |
| **B-02** | Thống nhất DTO `UserProfileDto` dùng chung cho Login, GetMe, UpdateProfile. | Application |
| **B-03** | `POST /auth/google` chỉ nhận `{ idToken }`, Backend tự verify qua `GoogleJsonWebSignature`. | API / Infra |
| **B-04** | Thống nhất cấu trúc `PagedResult<T>`, query param sắp xếp `sort=-createdAt`. | API / App |
| **B-05** | Chuẩn hóa HTTP Status Codes: `422` (Validation/Domain Error), `409` (Conflict/Concurrency), `401` (Unauthorized), `403` (Forbidden). | API / Middlewares |
| **B-06** | Bổ sung đầy đủ XML Documentation để Scalar API UI sinh tự động DTO Schema. | API |
| **B-07** | Bổ sung header `If-Match`, `ETag`, `X-Correlation-ID` vào CORS `Allow-Headers` và `Expose-Headers`. | API |
| **B-08** | Bỏ Presigned Upload cho tệp ảnh. Toàn bộ upload ảnh đi qua API Endpoint để kiểm tra Magic Bytes. | API / App |
| **B-09** | Server tự tính `StepNumber = Max + 1`. Bổ sung endpoint `PUT /recipes/{id}/steps/order` để đổi thứ tự các bước. | Application |
| **B-10** | Bổ sung 2 endpoints: `GET /api/v1/me/recipes` (bài của tôi) và `GET /api/v1/recipes/{id}` (xem chi tiết theo ID để sửa). | API / App |
| **B-11** | Quy định tiền tố `/api/v1` chỉ áp dụng cho Business APIs. `/health`, `/hangfire`, `/scalar` nằm ở root level. Cấm dùng slug là "search". | API |
| **C-01** | Chuyển `RowVersion` sang cột hệ thống `xmin` kiểu `xid` / `uint` trong Postgres. | Infrastructure |
| **C-02** | Sử dụng Raw SQL Migration tạo Extension `unaccent`, `pg_trgm` và Trigger FTS. | Infrastructure |
| **C-03** | Bật Output Cache cho Public Endpoints; Bỏ qua Output Cache cho Authenticated Endpoints. | API |
| **C-04** | Thống nhất 1 cơ chế Cache chính: Output Cache Redis Store với TTL 15 phút, Invalidate theo Tag. | API / Infra |
| **C-05** | Verify ID Token Google bằng `GoogleJsonWebSignature.ValidateAsync()`, kiểm tra `email_verified == true`. | Infrastructure |
| **C-06** | Sử dụng ASP.NET Core Identity `SignInManager.CheckPasswordSignInAsync(lockoutOnFailure: true)` kiểm tra `IsActive`. | Infrastructure |
| **C-07** | Áp dụng thời gian ân hạn 30 giây cho Refresh Token Rotation theo `FamilyId`. | Infrastructure |
| **C-08** | Cấu hình JWT ký bằng HS256 với Secret Key >= 256-bit lấy từ User Secrets / Environment Variables. | Infrastructure |
| **C-09** | Endpoint `POST /auth/logout` chấp nhận `AllowAnonymous`, chỉ cần `refreshToken` hợp lệ trong Body để thu hồi. | API / App |
| **C-10** | Đưa `ApplicationUser` sang `Infrastructure/Identity`. Layer `Domain` giữ 0 NuGet dependencies, `AuthorId` là `string`. | Architecture |
| **C-11** | Sửa `/health/ready` probe: Chỉ kiểm tra kết nối PostgreSQL. Redis lỗi chỉ ghi Log và fallback về DB. | API |
| **C-12** | Cấu hình Nginx Docker container bọc SSL termination và proxy pass chính xác sang API container port 8080. | Infrastructure |
| **C-13** | Sử dụng ASP.NET Core Rate Limiting Middleware bọc ngoài API. | API |
| **C-14** | Đọc Magic Bytes thực tế (JPEG, PNG, WebP: 12 bytes, AVIF) kiểm tra tệp ảnh. | Application |
| **C-15** | Sinh 3 phiên bản ảnh (`original`, `medium 800x600`, `thumb 300x300`, `og 1200x630`). Xóa ảnh = xóa toàn bộ folder chứa các phiên bản theo Prefix. | App / Infra |
| **C-16** | Chuyển việc sinh `sitemap.xml` sang Next.js App Router (`app/sitemap.ts`). Recurring Job Hangfire chuyển sang dọn dẹp Token & Ảnh rác. | Frontend / Job |
| **C-17** | Thiết lập Hangfire `WelcomeEmailJob` có `AutomaticRetry(Attempts = 3)` kết nối qua MailKit / Mailhog. | Infrastructure |
| **C-18** | Cấu hình Auth.js v5 BFF Proxy chuyển tiếp Bearer JWT mượt mà giữa Next.js RSC và Backend API. | Frontend |
| **C-19** | Cập nhật Dockerfile Frontend chạy Next.js Standalone Mode trên nền Node.js 20+ LTS Runtime. | Frontend / DevOps |
| **C-20** | Loại bỏ `.IncludeOwned()`. Dùng `AsSplitQuery()` hoặc Projections (`.Select()`) cho các truy vấn phức tạp. | Infrastructure |
| **C-21** | Đổi thứ tự `StepNumber` bằng thuật toán 2 bước trong 1 Transaction (hoặc khóa dòng `Recipe` bằng `FOR UPDATE`). | Application |
| **C-22** | Thêm Partial Unique Index `CREATE UNIQUE INDEX ON "RecipeImages"("RecipeId") WHERE "IsPrimary" = true;`. | Infrastructure |
| **C-23** | Bổ sung ngoại lệ cho `CONS-006`: Cho phép Raw SQL trong Migration dành riêng cho Extensions, Triggers và FTS Indexes. | Architecture |
| **C-24** | Thêm B-Tree Index cho `(Status, CreatedAt)`, `CategoryId`, `AuthorId` trong `Recipes`. | Infrastructure |
| **C-25** | Bọc luồng Tạo User + Gán Role + Tạo Refresh Token trong một `IDbContextTransaction`. | Infrastructure |
| **C-26** | `AvatarUrl` ở v1 chỉ nhận từ Google Profile hoặc để trống (không nhận URL tùy ý). | Application |
| **C-27** | User bị vô hiệu hóa hoặc bị xóa -> Trả về `401 Unauthorized` đồng bộ cho tất cả Authenticated Endpoints. | API / Middlewares |
| **C-28** | Cập nhật `Recipe.UpdatedAt` trong cùng Transaction khi có bất kỳ thay đổi nào ở `Steps`, `Ingredients`, `Images`. | Application |
| **C-29** | Áp dụng Resource-Based Authorization (`RecipeAuthorizationHandler`) cho tất cả thao tác Ghi (Sửa, Xóa, Publish, Archive). | Application |
| **C-30** | Lấy JWT Secret Key từ Environment Variables. Frontend không giải mã JWT mà chỉ truyền qua Bearer Header. | Infra / FE |
| **C-31** | Log Production ghi ra Console định dạng JSON (để Docker thu thập) và đẩy về Seq tập trung. Sử dụng Serilog `LogContext`. | Infrastructure |
| **C-32** | Cập nhật Sơ đồ Bối cảnh Hệ thống bao gồm đầy đủ Nginx, PostgreSQL, Redis, S3 Storage, Seq, Mailhog. | Architecture |
| **D-01** | Sửa lại con số SLA 99.5% uptime trong tài liệu thành `≈ 43.8 giờ downtime/năm`. | Documentation |
| **D-02 -> D-06** | Phân tầng NFR thành 2 mức: **Core Requirements** (kiểm thử trực tiếp trên demo) và **Extension Goals**. | Documentation |
| **E-01 -> E-06** | Cập nhật toàn bộ trích dẫn tiêu chuẩn kỹ thuật (RFC 9457, RFC 9110, ISO/IEC/IEEE 29148:2018), ghim phiên bản thư viện có giấy phép MIT (MediatR v12.x). | Documentation |
| **F-01** | Bổ sung Module Quên / Đặt lại Mật khẩu: `POST /auth/forgot-password` (gửi token qua email) và `POST /auth/reset-password`. | App / API |
| **F-02** | Bổ sung API Quản trị User cơ bản cho Admin: `PATCH /admin/users/{id}/status` (Khóa / Mở khóa tài khoản). | App / API |
| **F-03** | Bổ sung `GET /api/v1/me/recipes?status={Draft|Published|Archived}`. | App / API |
| **F-04** | Bổ sung State Diagram mô tả vòng đời bài viết và Traceability Matrix. | Documentation |
| **F-05** | Bổ sung Hangfire Recurring Jobs: Dọn dẹp Refresh Token hết hạn (chạy 00:00 UTC) và dọn file mồ côi S3 (chạy 02:00 UTC). | Infrastructure |
| **F-06** | Quy định tất cả các trường văn bản (`Description`, `Steps.Description`, `Notes`) là **Plain Text** (render giữ xuống dòng, escape HTML chống XSS). | App / FE |
| **F-07** | Bổ sung cấu hình Nginx Reverse Proxy chi tiết trong bộ khung dự án. | Infrastructure |
| **F-08** | Cấu hình Axios Interceptor ở Frontend tự động bắt lỗi `401` và gọi `/api/v1/auth/refresh` để duy trì phiên làm việc. | Frontend |

