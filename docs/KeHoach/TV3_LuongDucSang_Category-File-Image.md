# Kế hoạch cá nhân — TV3 · Lương Đức Sang

> **Module:** Danh mục (FR-CAT-001 → 005) · Lưu trữ file (FR-FILE-001/002) · Ảnh công thức (FR-RCP-008) · Job resize ảnh (FR-JOB-002)
> **Vai trò chung:** hạ tầng — Docker Compose, Hangfire, Nginx, Cloudflare Tunnel cho bản demo
> **Hạn chốt:** CN 01/11/2026 — ứng dụng hoàn thiện (đăng nhập, giao diện đầy đủ, web gần như hoàn chỉnh) · Xem mốc chung trong `00_KeHoach_TongThe.md`

## 1. Phạm vi trách nhiệm

| Nhóm | Nội dung |
|---|---|
| Hạ tầng | `docker-compose.yml` dev; cấu hình Hangfire dùng chung; Nginx một domain; Cloudflare Tunnel cho demo; script backup base |
| Backend module | `IFileStorage` + bản S3 và bản local; entity Category, RecipeImage; API danh mục; API ảnh (upload, cập nhật, xóa); job resize; job dọn file mồ côi |
| Frontend | Component upload ảnh (dùng trong wizard của TV2); `/dashboard/categories`; `/categories`; `/categories/[slug]` |
| Lỗi SRS phụ trách xử lý | B-06, B-07, C-08, D-14, D-15, D-16 (phần Hangfire), D-18, D-22, F-01, G-07; phần của B-01, B-04, B-05, D-03, D-04, G-03 liên quan danh mục/ảnh |

## 2. Quyết định áp dụng cho phần việc

- **Storage (S-09) và câu trả lời của GV về Cloudflare Tunnel:**
  - Code trung lập S3: `AWSSDK.S3` với `ServiceURL` + `ForcePathStyle`; cấu hình `PublicBaseUrl`; DB **chỉ lưu object key**.
  - Dev: container storage tương thích S3 **ghim phiên bản theo digest** (image MinIO cộng đồng cuối còn kéo được); không kéo được → dùng `LocalFileStorage`.
  - Demo: **Cloudflare Tunnel** đưa cả site ra Internet từ máy nhóm (không cần VPS, IP tĩnh). Tunnel **không thay thế** kho ảnh: ảnh được Nginx phục vụ qua `/media/…` nên đi chung đường Tunnel.
- **Ảnh (S-12, GV: không cần AVIF):** chỉ JPEG, PNG, WebP; upload qua API (không dùng presigned URL); tối đa 5 MB → 413; kiểm tra chữ ký file (JPEG `FF D8 FF`; PNG `89 50 4E 47 0D 0A 1A 0A`; WebP `RIFF`…`WEBP`) → sai 415; `Content-Type` và phần mở rộng lấy từ chữ ký.
- **Key storage:** `recipes/{recipeId}/{imageId}/original.{ext}`, `…/medium.webp`, `…/thumb.webp`. Xóa ảnh = xóa theo prefix `…/{imageId}/`; xóa thật recipe = xóa prefix `recipes/{recipeId}/`.
- **Ảnh chính:** ảnh đầu tiên tự là ảnh chính; partial unique index `("RecipeId") WHERE "IsPrimary"`; đổi ảnh chính trong một transaction; xóa ảnh chính → chọn ảnh có `OrderIndex` nhỏ nhất.
- **Ảnh không bắt buộc để publish** (GV) → xóa ảnh cuối cùng của recipe Published vẫn được phép.
- **Xóa lai (S-03):** Category xóa thật; bị chặn khi còn recipe ở **bất kỳ** trạng thái, **kể cả trong thùng rác** → 409 kèm số lượng. Ảnh của recipe trong thùng rác **chưa bị xóa** — chỉ xóa khi job dọn thùng rác của TV2 xóa thật.
- **Danh mục (S-11):** tên 2–100 ký tự, không trùng (không phân biệt hoa/thường) → 409 `CATEGORY_NAME_EXISTS`; slug tự thêm hậu tố; **slug không đổi khi đổi tên**; `recipeCount` chỉ đếm Published; sắp xếp theo `OrderIndex` rồi `Name`.
- **Cache (S-07):** bỏ `IMemoryCache` trong phân công cũ; command gọi `ICacheInvalidator` với tag `categories` (TV4 hiện thực cache thật).
- **Hangfire (S-14):** Hangfire.PostgreSql; `AutomaticRetry` toàn cục 3 lần, chờ 60/300/1800 giây; dashboard: dev chỉ localhost, demo bảo vệ bằng Basic Auth ở Nginx.
- **Docker (S-15):** ghim phiên bản mọi image; Redis có `requirepass`; Seq có `ACCEPT_EULA=Y` + mật khẩu admin; Mailpit thay MailHog; Nginx gọi `api:8080`.
- **Mã lỗi:** `CATEGORY_NOT_FOUND` (404), `CATEGORY_NAME_EXISTS` (409), `CATEGORY_DELETE_HAS_RECIPES` (409), `IMAGE_NOT_FOUND` (404), `FILE_SIZE_EXCEEDED` (413), `FILE_TYPE_NOT_ALLOWED` (415), `STORAGE_UNAVAILABLE` (503).

## 3. Mốc cá nhân

| Ngày | Mốc cá nhân | Gắn với mốc chung |
|---|---|---|
| CN 20/09 | Phương án storage dev + ADR storage, ảnh | M0 |
| T5 24/09 | `docker compose up` cho cả nhóm | — |
| T3 29/09 | Storage + Hangfire dùng được | M1 |
| CN 11/10 | API danh mục + API ảnh | M2 |
| CN 18/10 | Component upload + các trang danh mục | M3 |
| CN 25/10 | Job resize, Nginx, demo qua Cloudflare Tunnel | M4 |

## 4. Công việc chi tiết và deadline

**Mức ưu tiên:** M = Must · S = Should · C = Could (trễ thì chuyển sau 01/11).

### G0 — Chốt quyết định (T5 17/09 → CN 20/09)

| Mã | Công việc | Kết quả bàn giao / tiêu chí xong | Phụ thuộc | Deadline | Mức |
|---|---|---|---|---|---|
| 3.01 | Thử kéo image MinIO cộng đồng (và bản dự phòng); ghi lại digest chạy được, hoặc chốt dùng `LocalFileStorage` cho dev | Ghi chú kết quả trong ADR storage | — | T7 19/09 | M |
| 3.02 | ADR S-09 (storage trung lập S3 + `/media` qua Nginx + Cloudflare Tunnel cho demo) và ADR S-12 (định dạng, chữ ký file, key, biến thể ảnh) | 2 file ADR trong `docs/adr/` | 3.01 | CN 20/09 | M |

### G1 — Nền tảng (T2 21/09 → T4 30/09)

| Mã | Công việc | Kết quả bàn giao / tiêu chí xong | Phụ thuộc | Deadline | Mức |
|---|---|---|---|---|---|
| 3.03 | `docker-compose.yml` dev: PostgreSQL 16 (ghim), Redis 7 (`requirepass`), storage (ghim digest) + tạo bucket tự động, Mailpit, Seq (EULA + mật khẩu); volume; `.env.example`; README "chạy trong 5 phút" | **Bàn giao cho nhóm:** một người mới clone repo chạy `docker compose up -d` thành công | 3.01 | T5 24/09 | M |
| 3.04 | Entity Category (`Name`, `Slug`, `Description`, `ImageUrl` để trống, `OrderIndex`) + unique index không phân biệt hoa/thường + migration + seed khoảng 8 danh mục | **Bàn giao cho TV2:** bảng `Categories` có dữ liệu để Recipe tham chiếu | TV2 `BaseEntity` (25/09) | T7 26/09 | M |
| 3.05 | `IFileStorage` ở Application (`UploadAsync(Stream, key, contentType)`, `DeleteAsync(key)`, `DeleteByPrefixAsync(prefix)`, `GetPublicUrl(key)`) + `S3FileStorage` + `LocalFileStorage`; chọn bằng cấu hình | **Bàn giao cho TV2:** test upload/xóa theo prefix chạy với container storage | 3.03 | CN 27/09 | M |
| 3.06 | Báo cáo nghiên cứu Cloudflare Tunnel (1–2 trang): quick tunnel và named tunnel khác nhau thế nào, có cần domain trên Cloudflare không, chạy `cloudflared` trong Docker Compose, giới hạn khi demo, lưu ý bảo mật | File `docs/research/cloudflare-tunnel.md`; gửi nhóm đọc trước CN 27/09 | — | CN 27/09 | M |
| 3.07 | Hangfire: Hangfire.PostgreSql (schema riêng), `AutomaticRetry` toàn cục, `IBackgroundJobService` ở Application, dashboard chỉ bật ở Development | **Bàn giao cho TV1, TV2:** enqueue một job mẫu và thấy chạy trong dashboard | 3.03, TV2 khung solution (23/09) | T3 29/09 | M |

### G2a — Backend base (T5 01/10 → CN 11/10)

| Mã | Công việc | Kết quả bàn giao / tiêu chí xong | Phụ thuộc | Deadline | Mức |
|---|---|---|---|---|---|
| 3.08 | `GET /categories` (kèm `recipeCount` đếm Published chưa xóa) + `GET /categories/{slug}` (recipe Published, `PagedResult`, `pageSize` mặc định 12) | Test: danh mục rỗng, slug không tồn tại → 404, Draft không lọt vào danh sách | TV2 entity Recipe (27/09) | T7 03/10 | M |
| 3.09 | `POST`, `PUT`, `DELETE /categories` (Admin): validation, trùng tên → 409, slug hậu tố, slug giữ nguyên khi đổi tên, xóa bị chặn nếu còn recipe (kể cả thùng rác) → 409 kèm số lượng; gọi `ICacheInvalidator("categories")` | Test: Author gọi → 403; trùng tên khác hoa/thường → 409; xóa danh mục có recipe trong thùng rác → 409 | TV1 JWT + policy (29/09), TV4 `ICacheInvalidator` (30/09) | T3 06/10 | M |
| 3.10 | Entity RecipeImage (`OriginalKey`, `MediumKey`, `ThumbnailKey`, `AltText`, `IsPrimary`, `OrderIndex`) + partial unique index ảnh chính + migration | Không thể có 2 ảnh chính cho một recipe ở mức DB | TV2 entity Recipe (27/09) | T4 07/10 | M |
| 3.11 | `POST /recipes/{id}/images` (multipart: `file`, `altText?`): kiểm tra quyền chủ bài/Admin, 5 MB, chữ ký file, key theo quy ước, ảnh đầu là ảnh chính, cập nhật `Recipe.UpdatedAt`, enqueue job resize (tạm là job rỗng); trả `{ imageId, originalUrl, altText, isPrimary, orderIndex }` | Test: JPEG/PNG/WebP hợp lệ; file .jpg đổi đuôi từ .exe → 415; 6 MB → 413; người khác upload → 403 | 3.05, 3.07, 3.10 | T6 09/10 | M |
| 3.12 | `PATCH /recipes/{id}/images/{imageId}` (`altText`, `isPrimary`, `orderIndex`) + `DELETE` (xóa DB, enqueue `DeleteByPrefix`, gán lại ảnh chính) | Test: đổi ảnh chính → chỉ còn 1 ảnh chính; xóa ảnh chính → ảnh `OrderIndex` nhỏ nhất thành ảnh chính | 3.11 | CN 11/10 | M |

### G2b — Frontend base (T2 12/10 → CN 18/10)

| Mã | Công việc | Kết quả bàn giao / tiêu chí xong | Phụ thuộc | Deadline | Mức |
|---|---|---|---|---|---|
| 3.13 | Component upload ảnh: kéo thả nhiều file, kiểm tra loại/dung lượng phía client, thanh tiến trình (Axios `onUploadProgress`), chọn ảnh chính, nhập alt text, xóa, sắp xếp | **Bàn giao cho TV2:** component nhận `recipeId`, dùng được trong wizard và trang sửa | 3.11, 3.12, TV4 API client (25/09) | T5 15/10 | M |
| 3.14 | `/dashboard/categories` (Admin): danh sách, tạo, sửa, xóa; hiển thị lỗi 409 dễ hiểu | Author vào trang → bị chặn; Admin thao tác đủ | 3.09, TV1 Auth.js (02/10) | T6 16/10 | M |
| 3.15 | `/categories` và `/categories/[slug]`: lưới danh mục, danh sách recipe Published có phân trang; ISR | Trang hiển thị dữ liệu seed; phân trang hoạt động | 3.08, TV2 trang chi tiết (15/10) cho link | CN 18/10 | M |

### G3 — Tích hợp & base nâng cao (T2 19/10 → CN 25/10)

| Mã | Công việc | Kết quả bàn giao / tiêu chí xong | Phụ thuộc | Deadline | Mức |
|---|---|---|---|---|---|
| 3.16 | FR-JOB-002 job resize: tạo `thumb.webp` 300×300 (crop giữa) và `medium.webp` rộng tối đa 800 (giữ tỉ lệ); decode lỗi → đánh dấu ảnh lỗi, không retry vô hạn; cập nhật key; chọn thư viện (SkiaSharp hoặc NetVips) | Upload ảnh → vài giây sau có 2 biến thể trên storage; trang danh sách dùng thumbnail | 3.11 | T5 22/10 | M |
| 3.17 | Nginx một domain: `/` → web:3000, `/api/` → api:8080, `/media/` → bucket storage, `/hangfire` có Basic Auth, `/health` chỉ nội bộ; `PublicBaseUrl` = `/media` | Toàn bộ site chạy qua `http://localhost` không lỗi CORS | 3.03, TV4 Next.js | T6 23/10 | M |
| 3.18 | Cloudflare Tunnel cho demo: service `cloudflared` trong Compose (profile `demo`), hướng dẫn bật/tắt, kiểm tra đăng nhập, upload ảnh, xem ảnh qua URL công khai | Người ngoài mạng mở được site demo; ghi hướng dẫn trong README | 3.06, 3.17 | CN 25/10 | M |
| 3.19 | Recurring job dọn file mồ côi: prefix `recipes/{id}/{imageId}/` không còn bản ghi trong DB → xóa (bỏ qua recipe đang trong thùng rác) | Chạy thử với thư mục giả lập, không xóa nhầm ảnh của recipe trong thùng rác | 3.05, 3.07 | CN 25/10 | C |

### G4 — Kiểm thử, tài liệu, tổng duyệt (T2 26/10 → CN 01/11)

| Mã | Công việc | Kết quả bàn giao / tiêu chí xong | Phụ thuộc | Deadline | Mức |
|---|---|---|---|---|---|
| 3.20 | Hoàn thiện integration test danh mục và ảnh (sai định dạng, quá dung lượng, đổi ảnh chính, xóa danh mục bị chặn) | CI xanh | — | T4 28/10 | M |
| 3.21 | Script backup base: `pg_dump` + nén volume storage; hướng dẫn khôi phục | Chạy thử khôi phục thành công trên máy khác | 3.03 | T5 29/10 | M |
| 3.22 | Gửi TV1 phần SRS v1.1: 3.2, 3.5, FR-JOB-002, 5.3 (storage, Hangfire, Tunnel), 6.5, 7.5, 7.6, 8.2, 8.4, mã lỗi `CATEGORY_*`/`FILE_*`/`IMAGE_*` | Nội dung khớp OpenAPI thực tế | — | T5 29/10 | M |
| 3.23 | Tổng duyệt demo **qua Cloudflare Tunnel** (máy chạy demo, mạng, kịch bản dự phòng khi Tunnel lỗi: demo trên `localhost`) | Cả nhóm truy cập được link demo trong buổi tổng duyệt | 3.18 | T7 31/10 | M |
| 3.24 | Sửa lỗi cuối, gắn tag cùng nhóm | Tag `v1.0` | — | CN 01/11 | M |

## 5. Bàn giao và nhận

| Hướng | Nội dung | Với ai | Hạn |
|---|---|---|---|
| **Nhận** | Khung solution 4 tầng | TV2 | T4 23/09 |
| **Giao** | `docker-compose.yml` dev | Cả nhóm | T5 24/09 |
| **Nhận** | `AppDbContext` + `BaseEntity` | TV2 | T6 25/09 |
| **Giao** | Entity Category + seed | TV2 | T7 26/09 |
| **Giao** | `IFileStorage` (có `DeleteByPrefix`) | TV2 | CN 27/09 |
| **Nhận** | Entity Recipe + migration | TV2 | CN 27/09 |
| **Giao** | Hangfire + `IBackgroundJobService` | TV1, TV2 | T3 29/09 |
| **Nhận** | JWT + policy Admin | TV1 | T3 29/09 |
| **Nhận** | `ICacheInvalidator` | TV4 | T4 30/09 |
| **Giao** | Component upload ảnh | TV2 | T5 15/10 |
| **Giao** | Link demo qua Cloudflare Tunnel | Cả nhóm | CN 25/10 |
| **Giao** | Phần SRS v1.1 | TV1 | T5 29/10 |

## 6. Hướng phát triển sau 01/11

| Thứ tự | Hạng mục | Ghi chú |
|---|---|---|
| 1 | Job dọn file mồ côi (nếu 3.19 chưa làm) | Đã có thiết kế |
| 2 | Biến thể ảnh Open Graph 1200×630 cho SEO | Phối hợp TV4 |
| 3 | `docker-compose.prod.yml` đầy đủ: chỉ `expose`, không publish cổng DB/Redis/storage; healthcheck; giới hạn tài nguyên | Chuẩn bị triển khai thật |
| 4 | Upload ảnh đại diện danh mục; sắp xếp danh mục bằng kéo thả | Dùng lại component 3.13 |
| 5 | Chuyển recipe hàng loạt giữa các danh mục (phối hợp TV2) | Giải quyết B-07 triệt để |
| 6 | Kho ảnh lâu dài: đánh giá dịch vụ S3 khác còn được bảo trì hoặc Cloudflare R2; bật cache ảnh qua Cloudflare | Chỉ đổi cấu hình nhờ `IFileStorage` |
| 7 | Backup tự động theo lịch + kiểm tra khôi phục định kỳ | Mở rộng 3.21 |

## 7. Rủi ro, dự phòng và checklist

**Rủi ro:**

| Rủi ro | Dấu hiệu | Dự phòng |
|---|---|---|
| Image MinIO không còn kéo được | 3.01 thất bại | `LocalFileStorage` cho dev; Nginx phục vụ thư mục ảnh qua `/media` giống hệt |
| Cloudflare Tunnel cần domain hoặc tài khoản mà nhóm chưa có | 3.06 kết luận named tunnel cần domain | Dùng quick tunnel (URL ngẫu nhiên) cho buổi demo; ghi rõ giới hạn trong README |
| Thư viện ảnh khó chạy trong container Linux | 3.16 lỗi thiếu thư viện native | Đổi sang thư viện khác; nếu vẫn trễ → chuyển 3.16 sau 01/11, tạm hiển thị ảnh gốc |
| Component upload trễ làm chậm wizard TV2 | T3 13/10 chưa upload được từ giao diện | Giao trước bản tối giản (chọn file + upload + hiển thị), kéo thả và sắp xếp bổ sung sau |
| Khối lượng hạ tầng lấn phần module | CN 27/09 chưa xong 3.03–3.05 | Nhờ TV4 hỗ trợ README/`.env.example`; báo nhóm trong kênh chung ngày CN 27/09 |

**Checklist tự kiểm tra trước CN 01/11:**
- [ ] `docker compose up -d` chạy từ repo sạch, mọi image đã ghim phiên bản
- [ ] Redis có mật khẩu; Seq khởi động được; email hiện trong Mailpit
- [ ] Upload JPEG/PNG/WebP thành công; file giả mạo → 415; quá 5 MB → 413
- [ ] DB chỉ lưu key; URL ảnh đi qua `/media`
- [ ] Mỗi recipe tối đa 1 ảnh chính; xóa ảnh chính tự chọn ảnh khác
- [ ] Xóa ảnh thì xóa cả biến thể trên storage
- [ ] Danh mục trùng tên → 409; xóa danh mục còn recipe (kể cả thùng rác) → 409
- [ ] Job resize và job dọn dẹp hiện trong Hangfire; dashboard có bảo vệ
- [ ] Link demo Cloudflare Tunnel hoạt động; có phương án dự phòng
- [ ] SRS v1.1 phần 3.2, 3.5, 7.5–7.6, 8.2, 8.4 khớp OpenAPI
