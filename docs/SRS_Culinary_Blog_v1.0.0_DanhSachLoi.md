# DANH SÁCH LỖI & MÂU THUẪN — SRS CULINARY BLOG v1.0.0

**Tài liệu rà soát:** SRS_Culinary_Blog_v1.0.0.pdf / SRS_Culinary_Blog_v1.0.0.md  
---

## TÓM TẮT PHÂN LOẠI LỖI (NHÓM A — F)

| Nhóm | Nghiêm trọng | Cao | Trung bình | Thấp | Tổng |
| :--- | :---: | :---: | :---: | :---: | :---: |
| **A — Nghiệp vụ & Dữ liệu** | 1 | 7 | 3 | 0 | **11** |
| **B — Hợp đồng API** | 2 | 7 | 1 | 1 | **11** |
| **C — Kỹ thuật & Kiến trúc** | 5 | 14 | 9 | 4 | **32** |
| **D — Yêu cầu Phi chức năng (NFR)** | 0 | 1 | 3 | 2 | **6** |
| **E — Công nghệ & Thư viện** | 1 | 3 | 1 | 1 | **6** |
| **F — Thiếu sót Yêu cầu** | 0 | 1 | 6 | 1 | **8** |
| **TỔNG CỘNG** | **9** | **33** | **23** | **9** | **74** |

---

## BẢNG TỔNG HỢP 9 LỖI NGHIÊM TRỌNG (BLOCKING ISSUES)

| Mã | Vấn đề cốt lõi | Hệ quả kỹ thuật |
| :--- | :--- | :--- |
| **A-01** | Bất nhất giữa Hard Delete & Soft Delete | Xung đột CASCADE delete, làm hỏng UNIQUE index và sai đếm số lượng công thức |
| **B-01** | Hợp đồng `POST /auth/register` mâu thuẫn | Frontend và Backend không thể giao tiếp, không rõ luồng tự đăng nhập |
| **B-03** | Đăng nhập Google thiếu quy chuẩn Token | Lỗ hổng giả mạo token, không xác thực chữ ký Google OAuth |
| **C-01** | Dùng `RowVersion bytea [Timestamp]` trên PostgreSQL | EF Core Npgsql không hỗ trợ `[Timestamp]` tự tăng trên Postgres |
| **C-02** | Mô tả Full-Text Search PostgreSQL sai cú pháp | Lỗi runtime SQL, thiếu cấu hình `unaccent` và `tsvector` chuẩn |
| **C-03** | Output Cache bị lộ dữ liệu riêng tư (Draft) | Guest có thể xem được bài viết nháp (Draft) của Author do cache tĩnh |
| **C-04** | 4 cơ chế Cache với TTL chồng chéo, xung đột | Dữ liệu không bao giờ hết hạn hoặc mất tới 65 phút mới cập nhật bài mới |
| **C-05** | Lỗ hổng bảo mật nghiêm trọng trong Google Auth | Nhận thông tin giả mạo từ client mà không verify ID Token phía Backend |
| **E-01** | MinIO bản cộng đồng đã bị khai tử | `docker compose pull` thất bại do image minio bị gỡ khỏi Docker Hub |

---

## A. MÂU THUẪN NGHIỆP VỤ & MÔ HÌNH DỮ LIỆU

### A-01 · Nghiêm trọng · Xóa công thức/danh mục: hard delete hay soft delete?
* **Vị trí:** FR-RCP-007 (tr. 32–33); FR-CAT-005 (tr. 26–27); NFR-REL-003 (tr. 43); 6.4 (tr. 52); 7.1, 8.2, 8.3; Phụ lục A.
* **Nội dung:** FR-RCP-007 yêu cầu Hard Delete (xóa hẳn DB và xóa file MinIO), trong khi NFR-REL-003 và Chương 7 yêu cầu tất cả Entity dùng Soft Delete (`IsDeleted`).
* **Hệ quả:** `ON DELETE CASCADE` không chạy khi Soft Delete làm rác DB; ràng buộc UNIQUE (`Slug`, `Category.Name`) bị chiếm chỗ bởi bản ghi xóa mềm.

### A-02 · Cao · Điều kiện xuất bản (Publish) công thức bất nhất
* **Vị trí:** FR-RCP-005 (tr. 31) ↔ Phụ lục B (tr. 68).
* **Nội dung:** FR-RCP-005 chỉ yêu cầu `Steps.Count > 0`, nhưng Phụ lục B lại yêu cầu phải có cả Ingredients và Ảnh chính.

### A-03 · Cao · Vòng đời trạng thái công thức không đầy đủ
* **Vị trí:** FR-RCP-005, FR-RCP-006 (tr. 31–32); 7.2; 8.3.
* **Nội dung:** Tiêu đề FR-RCP-006 ghi Archive/Unarchive nhưng không có endpoint Unarchive. Bỏ ngỏ việc xử lý `PublishedAt` khi Unpublish.

### A-04 · Cao · Trùng Slug công thức giữa các tác giả
* **Vị trí:** FR-RCP-003 (tr. 29) ↔ 7.2 (tr. 56).
* **Nội dung:** SRS trả về lỗi `409 Conflict` khi trùng Slug nhưng không có cơ chế tự thêm hậu tố (`-1`, `-2`) hay cho phép tác giả khác đặt trùng tiêu đề.

### A-05 · Cao · Quản lý danh mục: Xóa danh mục đang chứa bài viết
* **Vị trí:** FR-CAT-005 (tr. 26–27) ↔ 8.2 (tr. 63).
* **Nội dung:** FR-CAT-005 yêu cầu kiểm tra danh mục rỗng trước khi xóa, nhưng không nêu rõ cơ chế chuyển danh mục hàng loạt hay báo lỗi `409`.

### A-06 · Cao · Quy tắc tên danh mục và slug danh mục
* **Vị trí:** FR-CAT-003/004 ↔ 7.6.
* **Nội dung:** Thiếu kiểm tra trùng tên Danh mục không phân biệt hoa/thường; độ dài tên giữa FR và DB lệch nhau (50 vs 100).

### A-07 · Cao · Hiển thị số lượng công thức trong danh mục (`recipeCount`)
* **Vị trí:** FR-CAT-001/002 ↔ 8.2.
* **Nội dung:** `recipeCount` bao gồm cả bài `Draft` và bài bị xóa mềm nếu đếm trực tiếp, làm lộ số lượng bài ẩn cho người dùng vô danh (Guest).

### A-08 · Trung bình · Ràng buộc kiểu dữ liệu và giá trị hợp lệ bất nhất
* **Vị trí:** Chương 3 ↔ Chương 7.
* **Nội dung:** `CookTimeMinutes` cho phép <= 0 hay không; `Difficulty` gồm Easy/Medium/Hard hay thêm Expert; `Quantity`/`Unit` có nullable cho nguyên liệu "vừa đủ" hay không.

### A-09 · Trung bình · Quy ước đặt tên trường dữ liệu lệch giữa C# và DB
* **Vị trí:** 7.1 – 7.8 ↔ Chương 8.
* **Nội dung:** C# DTO dùng PascalCase/camelCase (`fullName`), DB Postgres dùng snake_case (`full_name`), nhưng một số nơi viết trộn lẫn.

### A-10 · Trung bình · Dữ liệu và màn hình không có chức năng tương ứng
* **Vị trí:** 5.1; 7.3, 7.6, 7.7; Phụ lục B.
* **Nội dung:** Trang chủ hiển thị "công thức nổi bật" nhưng DB không có trường `IsFeatured`/`ViewCount`. Trường `Bio` của user không có trang tác giả để hiển thị.

### A-11 · Trung bình · Phân quyền: Vai trò và Policy không khớp nhau
* **Vị trí:** 2.3; FR-JOB-001; Chương 8.
* **Nội dung:** Mọi người đăng ký tự động là Author; Policy `VerifiedAuthor` yêu cầu xác nhận email nhưng không có FR xác nhận email. Guest xếp là role dù chưa đăng nhập.

---

## B. MÂU THUẪN HỢP ĐỒNG REST API

### B-01 · Nghiêm trọng · Hợp đồng đăng ký tài khoản khác nhau hoàn toàn
* **Vị trí:** FR-AUTH-001 (tr. 17–18) ↔ 8.1 (tr. 61).
* **Nội dung:** FR-AUTH-001 nhận `{fullName, email, userName, password}` và trả `AuthResponseDto` (tự đăng nhập). Mộc 8.1 nhận `{email, password, displayName}` và trả `{userId, email, displayName}` (không có token).

### B-02 · Cao · Response của login / me / cập nhật hồ sơ khác nhau
* **Vị trí:** FR-AUTH-002, 006, 007 ↔ 8.1.
* **Nội dung:** Login ở FR trả `expiresAt` + `user{...}`, ở 8.1 trả `expiresIn` thiếu object `user`. `GET /auth/me` trả danh sách trường lệch nhau giữa hai chương.

### B-03 · Nghiêm trọng · Đăng nhập Google được mô tả bằng 3 luồng khác nhau
* **Vị trí:** FR-AUTH-003 ↔ 5.3 ↔ 8.1.
* **Nội dung:** FR-AUTH-003 mô tả OAuth Code Flow với Auth.js v5; 5.3 mô tả callback redirect backend; 8.1 yêu cầu nhận `idToken` từ Google Sign-In JS SDK (đã bị khai tử).

### B-04 · Cao · Phân trang, sắp xếp và "vỏ bọc" response không thống nhất
* **Vị trí:** FR-RCP-001; FR-CAT-002; FR-SRCH-001; 5.2; Chương 8.
* **Nội dung:** Chương 3 dùng `PagedResult` trả thẳng; 5.2 bọc `{data, meta}`; Chương 8 bọc `{data:[], meta:{...}}`. Sắp xếp dùng `sort=-createdAt` hay `sortBy=createdAt&sortOrder=desc`.

### B-05 · Cao · Mã HTTP status mâu thuẫn và danh sách chưa đầy đủ
* **Vị trí:** Chương 3 ↔ Chương 8 ↔ Phụ lục A, B.
* **Nội dung:** Lỗi Validation dùng `422` (Chương 3) ↔ `400` (Chương 8). Concurrency conflict dùng `409` ↔ `422`. Thừa/thiếu các mã 401, 403, 423, 502.

### B-06 · Cao · Thiếu DTO định nghĩa Request/Response cho các endpoint
* **Vị trí:** Chương 8 (tr. 61–66).
* **Nội dung:** Bảng 8.3 (Recipes) bị vỡ layout, thiếu hẳn cột Response DTO.

### B-07 · Cao · Hợp đồng cập nhật công thức và vấn đề CORS
* **Vị trí:** FR-RCP-004; FR-RCP-009; 8.3; 5.2.
* **Nội dung:** FR-RCP-004 dùng PUT gửi body đầy đủ + header `If-Match`; 8.3 dùng PUT như PATCH (gửi trường tùy chọn). CORS Headers ở 5.2 thiếu `If-Match` trong `Allow-Headers` và thiếu `ETag` trong `Expose-Headers`.

### B-08 · Cao · Upload ảnh: Gửi `multipart/form-data` qua API hay Presigned URL?
* **Vị trí:** FR-RCP-008; FR-FILE-001 ↔ 5.3.
* **Nội dung:** FR-RCP-008 upload qua API .NET; 5.3 đề xuất Presigned URL đẩy thẳng lên MinIO làm bỏ qua toàn bộ bước kiểm tra magic bytes ở server.

### B-09 · Cao · API bước thực hiện: Ai quyết định `StepNumber`?
* **Vị trí:** FR-RCP-010 ↔ 8.5.
* **Nội dung:** FR quy định Server tự tính `StepNumber = Max + 1`; 8.5 bắt Client phải gửi `stepNumber`.

### B-10 · Trung bình · Thiếu các Endpoint cá nhân và quản trị
* **Vị trí:** Chương 8.
* **Nội dung:** Thiếu `GET /me/recipes` (xem danh sách bài của tôi) và `GET /recipes/{id}` (xem chi tiết bài nháp để sửa).

### B-11 · Thấp · Tiền tố `/api/v1` và xung đột route
* **Vị trí:** 5.2; 8.7; FR-SRCH-001; FR-RCP-002.
* **Nội dung:** `/health`, `/hangfire`, `/scalar` nằm ngoài tiền tố `/api/v1`. Xung đột route giữa `GET /recipes/search` và `GET /recipes/{slug}` nếu slug là "search".

---

## C. LỖI KỸ THUẬT & KIẾN TRÚC

### C-01 · Nghiêm trọng · RowVersion bytea `[Timestamp]` không hoạt động trên PostgreSQL
* **Vị trí:** FR-RCP-004; 6.4; 7.1; 7.2.
* **Nội dung:** Cấu hình `[Timestamp]` / `IsRowVersion()` thuộc SQL Server. Trên PostgreSQL (Npgsql), EF Core bắt buộc dùng cột hệ thống `xmin` kiểu `uint`.

### C-02 · Nghiêm trọng · Full-text search tiếng Việt viết sai về PostgreSQL
* **Vị trí:** FR-SRCH-001; 2.4.1; 7.2.
* **Nội dung:** PostgreSQL không có sẵn cấu hình `vietnamese` mặc định. Cột `SearchVector` dùng `EF.Functions.ToTsQuery` cần extension `unaccent` và custom Text Search Configuration.

### C-03 · Nghiêm trọng · Cache response nhưng nội dung lại phụ thuộc người xem
* **Vị trí:** FR-RCP-001, FR-RCP-002; FR-CAT-002; 5.1.
* **Nội dung:** Output Cache lưu response theo URL. Guest không được xem Draft nhưng Author phải xem được Draft của mình. Cache tĩnh sẽ gây lộ bài nháp cho Guest hoặc trả 403 cho Author.

### C-04 · Nghiêm trọng · Bốn cơ chế cache với TTL mâu thuẫn nhau
* **Vị trí:** FR-CAT-001; FR-RCP-001/002; FR-SRCH-001; NFR-PERF-003; 5.1; 6.3.
* **Nội dung:** `IMemoryCache` (60p) ↔ Redis (30p); Output Cache (60p) ↔ Redis cache-aside (5p) ↔ Next.js ISR (300s). Dữ liệu có thể mất hơn 1 tiếng mới cập nhật.

### C-05 · Nghiêm trọng · Lỗ hổng trong luồng đăng nhập Google
* **Vị trí:** FR-AUTH-003; 8.1.
* **Nội dung:** Frontend gửi thông tin Google profile sang backend mà backend không tự verify ID Token (`GoogleJsonWebSignature`). Kẻ tấn công có thể giả mạo email bất kỳ để chiếm tài khoản.

### C-06 · Cao · Cơ chế khóa tài khoản khi đăng nhập sai không hoàn chỉnh
* **Vị trí:** FR-AUTH-002.
* **Nội dung:** Thiếu đếm số lần sai trong Redis/Identity và không kiểm tra cờ `IsActive` khi đăng nhập.

### C-07 · Cao · Refresh token rotation bị race condition khi mở nhiều tab
* **Vị trí:** FR-AUTH-004; NFR-SEC-002.
* **Nội dung:** Thu hồi token ngay lập tức làm các request song song từ nhiều tab bị ngắt kết nối đồng loạt (ngăn cản hợp lệ bị tính là Reuse Attack).

### C-08 · Cao · Cấu hình JWT Token Security chưa đầy đủ
* **Vị trí:** NFR-SEC-002; 6.2.
* **Nội dung:** Dùng thuật toán HS256 với secret key cứng, thiếu cơ chế xoay khóa (key rotation) và validate `jti` (JWT ID).

### C-09 · Cao · Logout tự mâu thuẫn về access token hết hạn
* **Vị trí:** FR-AUTH-005; 8.1.
* **Nội dung:** Yêu cầu Header Bearer Token hợp lệ để Logout, nhưng luồng phụ cho phép Logout khi Access Token đã hết hạn (trả lỗi 401 trước khi đến Handler).

### C-10 · Cao · SRS tự vi phạm Clean Architecture mà chính nó quy định
* **Vị trí:** CONS-001; NFR-MAINT-004; 6.2; 6.3; 6.4; 7.7.
* **Nội dung:** Layer `Domain` phụ thuộc gói `ASP.NET Core Identity` (`ApplicationUser : IdentityUser`). Layer `Application` gọi trực tiếp `IFormFile`, `UserManager`, `Hangfire`.

### C-11 · Cao · Readiness probe làm sập hệ thống khi Redis lỗi
* **Vị trí:** FR-OBS-001; 8.7; 2.6.2; NFR-REL-002.
* **Nội dung:** Readiness probe đánh dấu Unhealthy làm Nginx dừng chuyển traffic khi Redis down, dù hệ thống có cơ chế fallback về PostgreSQL.

### C-12 · Cao · Nginx Reverse Proxy cấu hình thiếu SSL & Healthcheck
* **Vị trí:** 2.1.2; 6.5; 5.2.
* **Nội dung:** Nginx mã nguồn mở không có active health check tự động; thiếu cấu hình Let's Encrypt / SSL Certbot.

### C-13 · Cao · Rate Limiting trong môi trường Distributed/Docker
* **Vị trí:** NFR-SEC-003 ↔ 6.1.
* **Nội dung:** Rate limiting lưu in-memory không đồng bộ được giữa nhiều instance API nếu không dùng Redis store.

### C-14 · Cao · Kiểm tra file upload và lưu trữ ảnh có lỗ hổng
* **Vị trí:** CONS-007; FR-RCP-008; FR-FILE-001/002; NFR-SEC-004.
* **Nội dung:** Đọc 4 bytes đầu không đủ kiểm tra Magic Bytes cho WebP (cần 12 bytes) và AVIF. Tin tưởng header `Content-Type` do client gửi.

### C-15 · Cao · Phiên bản ảnh và việc dọn file bị bỏ sót
* **Vị trí:** FR-JOB-002; FR-RCP-007/008; NFR-SEO-002.
* **Nội dung:** FR-JOB-002 tạo ảnh Medium và Thumbnail nhưng khi xóa chỉ xóa ảnh gốc, để lại ảnh mồ côi trên S3. Thiếu kích thước ảnh OpenGraph `1200x630`.

### C-16 · Cao · Sitemap generation job (FR-JOB-003) thiết kế chưa đúng
* **Vị trí:** FR-JOB-003; NFR-SEO-003.
* **Nội dung:** Gửi ping sang Google Sitemap Ping API (Google đã khai tử endpoint này từ 06/2023).

### C-17 · Cao · Email chào mừng (FR-JOB-001) thiếu xử lý Retry & Template
* **Vị trí:** FR-JOB-001; 5.3.
* **Nội dung:** Không cấu hình template HTML riêng và thiếu cấu hình Retry rõ ràng trong Hangfire.

### C-18 · Cao · Next.js Frontend Auth & Server-side Fetching
* **Vị trí:** 5.1; 6.1.
* **Nội dung:** Auth.js v5 (beta) mâu thuần trong việc truyền JWT Token giữa Server Components (RSC) và Client Components.

### C-19 · Cao · "Node.js chỉ cần lúc build" là sai
* **Vị trí:** 2.4.1.
* **Nội dung:** Tuyên bố Node.js chỉ cần lúc build, trong khi Next.js Standalone mode bắt buộc phải có Node.js Runtime để chạy SSR/ISR ở production.

### C-20 · Trung bình · EF Core / Npgsql: API không tồn tại và cấu hình connection pool
* **Vị trí:** FR-RCP-002; NFR-PERF-004; NFR-SCALE-002.
* **Nội dung:** Gọi phương thức `.IncludeOwned()` không tồn tại trong EF Core. Cấu hình connection pool vượt quá `max_connections` của Postgres.

### C-21 · Trung bình · Đánh số lại bước thực hiện đụng ràng buộc UNIQUE
* **Vị trí:** FR-RCP-010; 7.3.
* **Nội dung:** Cập nhật `StepNumber` trên bảng có `UNIQUE(RecipeId, StepNumber)` ném ngoại lệ vi phạm constraint trước khi hoàn tất `SaveChangesAsync`.

### C-22 · Trung bình · "Chỉ 1 ảnh chính" không được đảm bảo ở DB
* **Vị trí:** 7.5; FR-RCP-008.
* **Nội dung:** Không có Partial Unique Index (`WHERE "IsPrimary" = true`), dẫn đến 2 request đồng thời có thể tạo ra 2 ảnh chính.

### C-23 · Trung bình · CONS-006 cấm raw SQL nhưng hệ thống bắt buộc phải có
* **Vị trí:** CONS-006; FR-SRCH-001; 2.4.1.
* **Nội dung:** Cấm Raw SQL ngăn cản việc viết Migration cho Extension (`unaccent`, `pg_trgm`) và Triggers/Indexes của Full-Text Search.

### C-24 · Trung bình · Bảng `Recipes` thiếu Index cho các truy vấn lọc thường dùng
* **Vị trí:** 7.2 ↔ FR-RCP-001.
* **Nội dung:** Thiếu B-Tree Index trên các cột `(Status, CreatedAt)`, `CategoryId`, `AuthorId`.

### C-25 · Trung bình · Đăng ký tài khoản không có Transaction
* **Vị trí:** FR-AUTH-001.
* **Nội dung:** Tạo User, gán Role, lưu Refresh Token chạy không có `IDbContextTransaction`, dễ gây rác dữ liệu khi lỗi giữa chừng.

### C-26 · Trung bình · Hồ sơ: AvatarUrl tự do và quy trình OTP "ma"
* **Vị trí:** FR-AUTH-007; 7.7.
* **Nội dung:** Cho phép nhập `AvatarUrl` tùy ý gây nguy cơ XSS/Mixed Content; nhắc tới "quy trình OTP đổi email" nhưng không đặc tả.

### C-27 · Thấp · Cùng một tình huống nhưng trả mã lỗi khác nhau
* **Vị trí:** FR-AUTH-004 A4 ↔ FR-AUTH-006 A1.
* **Nội dung:** User bị xóa: Refresh token trả `401`, nhưng `GET /auth/me` trả `404`.

### C-28 · Trung bình · Sửa các bảng con không làm thay đổi `RowVersion` của Recipe
* **Vị trí:** 3.3; FR-RCP-008/009/010.
* **Nội dung:** Sửa Steps, Ingredients, Images không cập nhật `UpdatedAt`/`xmin` của `Recipe`, khiến Concurrency Control bị qua mặt.

### C-29 · Thấp · Phạm vi kiểm tra quyền sở hữu mô tả thiếu
* **Vị trí:** NFR-SEC-006; FR-RCP-002.
* **Nội dung:** NFR chỉ ghi kiểm tra quyền khi "xóa", bỏ sót thao tác sửa, publish, archive.

### C-30 · Thấp · HS256 kết hợp xoay khóa 90 ngày
* **Vị trí:** NFR-SEC-002, 007.
* **Nội dung:** Khuyên xoay khóa 90 ngày nhưng không mô tả cơ chế hỗ trợ nhiều key active (`kid`) gây ngắt kết nối toàn bộ user khi đổi key.

### C-31 · Thấp · Ghi log production: Mâu thuẫn giữa Console, File, Seq
* **Vị trí:** FR-OBS-002; 5.3; 6.1; NFR-PERF-004.
* **Nội dung:** Mâu thuẫn nơi lưu log production; nhầm lẫn thuật ngữ Serilog `LogContext` thành "MDC" (log4j).

### C-32 · Thấp · Sơ đồ kiến trúc & Bối cảnh thiếu các dịch vụ phụ trợ
* **Vị trí:** 2.1.1; 6.1; 6.5.
* **Nội dung:** Sơ đồ thiếu Nginx, Mailhog/SMTP, Seq trong luồng kết nối chính.

---

## D. YÊU CẦU PHI CHỨC NĂNG (NFR)

### D-01 · Cao · Tính sai thời gian downtime của SLA 99.5%
* **Vị trí:** NFR-REL-001 (tr. 43).
* **Nội dung:** SRS ghi `99.5% uptime ≈ 3.65 giờ downtime/năm`. Con số đúng phải là `0.5% × 8760 giờ ≈ 43.8 giờ downtime/năm`.

### D-02 · Trung bình · Chỉ tiêu hiệu năng tự mâu thuẫn và không kiểm chứng được
* **Vị trí:** NFR-PERF-001/002/003.
* **Nội dung:** Yêu cầu `p99 <= 1000ms` "trong mọi trường hợp" (kể cả upload file 5MB) mâu thuẫn với định nghĩa thống kê p99; yêu cầu đo đạc trên production thực tế là phi thực tế với đồ án.

### D-03 · Trung bình · Yêu cầu bảo trì quá nặng so với nhóm 4 sinh viên
* **Vị trí:** NFR-MAINT-001/002/003.
* **Nội dung:** Yêu cầu StyleCop + SonarAnalyzer + 80% coverage + E2E Playwright + ADR cho mọi quyết định là quá tải; ESLint Airbnb ruleset không tương thích tốt với Next.js 15/16.

### D-04 · Trung bình · SEO đòi hỏi thứ nằm ngoài phạm vi
* **Vị trí:** NFR-SEO-001/002.
* **Nội dung:** Đòi hỏi điểm Lighthouse SEO >= 95 và Core Web Vitals xanh tuyệt đối khi chưa có môi trường staging chuẩn.

### D-05 · Thấp · Khả năng sử dụng (Usability) thiếu kịch bản test cụ thể
* **Vị trí:** NFR-USE-001/002/004.
* **Nội dung:** Đặt chuẩn WCAG 2.1 AA nhưng không có công cụ kiểm thử tự động (Axe/Lighthouse) trong CI pipeline.

### D-06 · Thấp · Yêu cầu phần cứng mâu thuẫn và phi thực tế
* **Vị trí:** 2.4.1 ↔ 5.4.
* **Nội dung:** Dung lượng đĩa Production ghi 20GB ở 2.4.1 nhưng ghi 50GB ở 5.4. Yêu cầu "IP tĩnh, Băng thông 1Gbps" cho VPS demo là phi thực tế.

---

## E. CÔNG NGHỆ, THƯ VIỆN & RỦI RO

### E-01 · Nghiêm trọng · MinIO bản cộng đồng đã bị khai tử
* **Vị trí:** 2.1.2; 2.4.1; 2.6.2; 6.5.
* **Nội dung:** MinIO đã ngừng phát hành bản binary/Docker image cộng đồng từ cuối 2025. Image `minio/minio:latest` trên Docker Hub không còn tồn tại hoặc không được cập nhật.

### E-02 · Cao · Danh sách trình duyệt hỗ trợ mâu thuẫn và không khớp với framework
* **Vị trí:** 2.4.3 ↔ 5.4.
* **Nội dung:** Cả 2 danh sách trình duyệt trong SRS đều quá cũ, không tương thích với yêu cầu tối thiểu của Next.js 15/16 và Tailwind CSS v4.

### E-03 · Cao · Auth.js v5 (beta) nâng cấp breaking changes
* **Vị trí:** 6.1; 6.2.
* **Nội dung:** Dự án Auth.js v5 bị đóng băng/sáp nhập sang Better Auth; dùng bản v5 beta gây rủi ro lớn về tính ổn định.

### E-04 · Trung bình · Công cụ phát triển (.NET 10 SDK & Node.js EOL)
* **Vị trí:** 2.4.2.
* **Nội dung:** VS 2022 v17.12 không hỗ trợ .NET 10 (phải dùng VS 2026 / v18+ hoặc Rider/VS Code). Node.js 20 LTS đã hết hạn hỗ trợ (EOL).

### E-05 · Trung bình · MediatR >= 13 chuyển sang giấy phép thương mại
* **Vị trí:** CONS-002; 6.3.
* **Nội dung:** MediatR v13+ yêu cầu License Key thương mại. Cần ghim bản v12.x (MIT) hoặc đăng ký Community License giáo dục.

### E-06 · Thấp · Trích dẫn tiêu chuẩn cũ đã bị thay thế
* **Vị trí:** 1.1, 1.4, CONS-005, Phụ lục C.
* **Nội dung:** RFC 7807 đã bị thay thế bởi **RFC 9457**; RFC 7231 đã bị thay thế bởi **RFC 9110**.

---

## F. THIẾU SÓT YÊU CẦU CHỨC NĂNG & VẬN HÀNH

### F-01 · Cao · Không có chức năng Quên / Đặt lại mật khẩu và Xác nhận Email
* **Vị trí:** 3.1 (tr. 17–23).
* **Nội dung:** Có chức năng khóa tài khoản khi nhập sai 5 lần nhưng hoàn toàn **không có** luồng Quên mật khẩu / Đặt lại mật khẩu qua email token.

### F-02 · Trung bình · Không có chức năng Quản trị người dùng (User Management)
* **Vị trí:** 2.3; 7.7; Phụ lục B.
* **Nội dung:** Admin không có giao diện/API để xem danh sách người dùng, Khóa/Mở khóa (`IsActive`), hoặc gán Role.

### F-03 · Trung bình · Thiếu chức năng Quản lý bài viết cá nhân (`/me/recipes`)
* **Vị trí:** 3.3.
* **Nội dung:** Tác giả không có endpoint riêng để xem toàn bộ bài viết của mình (bao gồm cả Draft, Archived) với bộ lọc trạng thái.

### F-04 · Trung bình · Thiếu ma trận truy vết (Traceability Matrix) & State Diagram
* **Vị trí:** Toàn tài liệu.
* **Nội dung:** Thiếu ma trận ánh xạ FR ↔ API Endpoint ↔ DB Table; thiếu sơ đồ chuyển trạng thái (State Diagram) của `Recipe`.

### F-05 · Trung bình · Thiếu quy trình Backup, Cleanup Jobs & Seeding Data
* **Vị trí:** 4.4; 5.3; 6.5.
* **Nội dung:** Thiếu Background Job dọn dẹp Refresh Token hết hạn, dọn file ảnh mồ côi trên S3; thiếu mật khẩu mặc định cho Admin Seeding.

### F-06 · Trung bình · Không định nghĩa định dạng nội dung (Plain Text / Markdown / HTML)
* **Vị trí:** 7.2; FR-RCP-003/010; NFR-SEC-004.
* **Nội dung:** Không quy định rõ các trường `Description`, `Steps.Description` là Plain Text, Markdown hay HTML, gây khó khăn cho việc sanitize XSS và render UI.

### F-07 · Trung bình · Chưa định nghĩa Domain & Định tuyến Nginx chi tiết
* **Vị trí:** 5.2; NFR-SEC-005; 6.1, 6.5.
* **Nội dung:** Thiếu file cấu hình Nginx chi tiết định tuyến `/api`, `/hangfire`, `/scalar`, `/health` và media files.

### F-08 · Thấp · Thiếu đặc tả xử lý Token Refresh & Error Boundary ở Frontend
* **Vị trí:** 5.1; 6.1.
* **Nội dung:** Thiếu quy trình tự động gọi API Refresh Token ở Axios Interceptor / TanStack Query khi gặp lỗi 401 tại Frontend.
