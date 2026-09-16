# Kế hoạch cá nhân — TV4 · Trần Lê Bảo Thư

> **Module:** Danh sách công thức (FR-RCP-001) · Tìm kiếm, lọc, sắp xếp, phân trang (FR-SRCH-001 → 004) · SEO (NFR-SEO) · Sitemap (thay FR-JOB-003) · Observability (FR-OBS-001, 002)
> **Vai trò chung:** nền frontend (khung Next.js, API client), CI, dữ liệu seed, đo kiểm NFR base
> **Hạn chốt:** CN 01/11/2026 — ứng dụng hoàn thiện (đăng nhập, giao diện đầy đủ, web gần như hoàn chỉnh) · Xem mốc chung trong `00_KeHoach_TongThe.md`

## 1. Phạm vi trách nhiệm

| Nhóm | Nội dung |
|---|---|
| Nền frontend & CI | Khung Next.js 16 + Tailwind 4 + TanStack Query; API client hai base URL; proxy `/api`; sinh kiểu TypeScript từ OpenAPI; GitHub Actions; Playwright |
| Backend module | Serilog + CorrelationId + `LoggingBehavior`; health checks; `ICacheInvalidator` và Output Cache; `GET /recipes` công khai; tìm kiếm không dấu bằng `pg_trgm`; dữ liệu seed |
| Frontend | `/` (trang chủ), `/recipes`, `/search`; metadata, Open Graph, JSON-LD cho `/recipes/[slug]`; `sitemap.xml`, `robots.txt` |
| Đo kiểm | k6, Lighthouse, Google Rich Results Test theo NFR base |
| Lỗi SRS phụ trách xử lý | C-04, D-02, D-03, D-04, D-11, D-12, D-17, D-23, D-24, D-31, E-01, E-02, E-04, E-05, F-02, G-05 (phần log); phần của A-02, B-02, B-04, B-05, C-11 liên quan tìm kiếm/SEO |

## 2. Quyết định áp dụng cho phần việc

- **Tìm kiếm (S-08 — nhóm đã chốt phương án B, cho phép tiếng Việt không dấu):**
  - Extension `unaccent` + `pg_trgm`; hàm bọc `f_unaccent` khai báo `IMMUTABLE`.
  - Cột `SearchText` (generated, stored) = `lower(f_unaccent(Title || ' ' || Description))`; GIN index `gin_trgm_ops`.
  - Truy vấn: chuẩn hóa `q` (bỏ dấu, chữ thường, ≥ 2 ký tự) → điều kiện `ILIKE '%q%'` **hoặc** độ tương đồng trigram vượt ngưỡng → sắp theo độ tương đồng (ưu tiên khớp Title). Dùng `EF.Functions.ILike` và gói trigram của Npgsql, hoặc `FromSql` có tham số.
  - Chỉ trả recipe Published và chưa xóa; áp dụng được các bộ lọc giống danh sách.
  - SQL trong migration được phép (ngoại lệ CONS-006).
- **Danh sách (S-10, S-11):** chỉ Published + chưa xóa; lọc `categoryId`, `difficulty`, `maxCookTime`, `minServings`; `sort` thuộc whitelist `createdAt`, `publishedAt`, `title`, `cookTimeMinutes` (tiền tố `-` = giảm dần, mặc định `-publishedAt`); `page` ≥ 1, `pageSize` mặc định 12, tối đa 50; trả `PagedResult<T>`.
- **Cache (S-07):** Output Cache + Redis store cho GET công khai; TTL: danh mục 30 phút, danh sách 5 phút, chi tiết 5 phút, tìm kiếm 1 phút (absolute); tag `categories`, `recipes`, `recipe:{id}`; policy mặc định không cache request có xác thực.
- **Observability base (S-15, S-16):** Serilog → Console JSON + Seq; `X-Correlation-ID`; enrich `UserId`, `RequestPath`; cảnh báo request > 500 ms; `/health` (DB, Redis, storage — Redis/storage lỗi báo Degraded), `/health/live`, `/health/ready` (**chỉ DB**). OpenTelemetry tracing/metrics (FR-OBS-003) chuyển sang **mở rộng**.
- **SEO base (S-11, S-14, E-04):** `generateMetadata` (title ≤ 60 ký tự, description ≤ 160, canonical); Open Graph/Twitter dùng ảnh medium; JSON-LD `Recipe` (bỏ `image` khi recipe không có ảnh — GV cho phép publish không ảnh); không có "star rating"; sitemap bằng `app/sitemap.ts`, `app/robots.ts`; **không** ping Google.
- **Trang chủ:** "nổi bật" = recipe mới publish gần nhất có ảnh; nếu chưa có ảnh nào thì lấy recipe mới nhất.
- **Phiên bản & trình duyệt (S-18):** Next.js 16.x (ghim), Node 22/24 LTS, Tailwind 4; trình duyệt Chrome/Edge 111+, Firefox 128+, Safari 16.4+; ESLint `eslint-config-next` + Prettier.
- **NFR base (GV: làm base trước):** GET công khai p95 ≤ 500 ms với k6 50 VU trong 5 phút trên máy demo; Lighthouse mobile trang chi tiết LCP ≤ 2,5 s, CLS ≤ 0,1; Rich Results Test không lỗi với recipe có ảnh.
- **Mã lỗi:** `SEARCH_QUERY_TOO_SHORT` (422), `VALIDATION_ERROR` (422) cho tham số lọc/sắp xếp sai.

## 3. Mốc cá nhân

| Ngày | Mốc cá nhân | Gắn với mốc chung |
|---|---|---|
| T6 18/09 | Repo + bảng Kanban cho cả nhóm | — |
| CN 20/09 | ADR cache, tìm kiếm, NFR, phiên bản, sitemap | M0 |
| T6 25/09 | Khung Next.js cho cả nhóm | — |
| CN 27/09 | CI chạy trên mọi PR | — |
| T4 30/09 | Log + health + `ICacheInvalidator` | M1 |
| CN 11/10 | API danh sách + tìm kiếm + dữ liệu seed | M2 |
| CN 18/10 | Trang chủ, danh sách, tìm kiếm | M3 |
| CN 25/10 | Cache, SEO, sitemap, Playwright | M4 |
| T5 29/10 | Báo cáo đo NFR base | M5 |

## 4. Công việc chi tiết và deadline

**Mức ưu tiên:** M = Must · S = Should (trễ thì chuyển sau 01/11).

### G0 — Chốt quyết định (T5 17/09 → CN 20/09)

| Mã | Công việc | Kết quả bàn giao / tiêu chí xong | Phụ thuộc | Deadline | Mức |
|---|---|---|---|---|---|
| 4.01 | ADR S-07 (cache), S-08 (tìm kiếm trigram không dấu), S-14 (sitemap bằng Next.js + job dọn dẹp), S-16 (NFR base/mở rộng), S-18 (phiên bản, trình duyệt) | 5 file ADR ngắn trong `docs/adr/` | TV1 mẫu ADR | CN 20/09 | M |
| 4.02 | Tạo repo/nhánh `main` được bảo vệ, template Pull Request, GitHub Projects (Kanban) với các mã công việc của 4 file kế hoạch | **Bàn giao cho nhóm:** cả nhóm clone được repo và thấy bảng công việc (TV1, TV2 cần repo để đưa ADR lên) | — | T6 18/09 | M |

### G1 — Nền tảng (T2 21/09 → T4 30/09)

| Mã | Công việc | Kết quả bàn giao / tiêu chí xong | Phụ thuộc | Deadline | Mức |
|---|---|---|---|---|---|
| 4.03 | Khung Next.js 16 + TypeScript + Tailwind 4 + TanStack Query; API client (`API_INTERNAL_URL` phía server, `NEXT_PUBLIC_API_BASE_URL` phía trình duyệt) có chỗ gắn token (TV1 nối Auth.js); `rewrites` proxy `/api`; layout chung (header, footer, trang 404/500); ESLint + Prettier | **Bàn giao cho TV1, TV2, TV3:** `npm run dev` gọi được một endpoint health của API | — | T6 25/09 | M |
| 4.04 | GitHub Actions: `dotnet build` + `dotnet test`; `npm ci` + lint + build; script sinh kiểu TypeScript từ OpenAPI (`openapi-typescript`) | **Bàn giao cho nhóm:** PR đỏ khi build/test lỗi | TV2 khung solution (23/09) | CN 27/09 | M |
| 4.05 | Serilog (Console JSON + Seq), `CorrelationIdMiddleware`, enrich `UserId`/`RequestPath`, `LoggingBehavior` (log tên request, thời gian, cảnh báo > 500 ms) | Mở Seq thấy log có `CorrelationId`; không log body chứa mật khẩu | TV2 khung MediatR (26/09), TV3 Seq (24/09) | T3 29/09 | M |
| 4.06 | Health checks `/health`, `/health/live`, `/health/ready` theo quyết định; `ICacheInvalidator` ở Application + bản rỗng (no-op) | **Bàn giao cho TV2, TV3:** gọi `InvalidateAsync(tags)` được ngay; tắt Redis → `/health/ready` vẫn 200 | TV3 compose | T4 30/09 | M |

### G2a — Backend base (T5 01/10 → CN 11/10)

| Mã | Công việc | Kết quả bàn giao / tiêu chí xong | Phụ thuộc | Deadline | Mức |
|---|---|---|---|---|---|
| 4.07 | `GET /recipes` công khai: lọc, sắp xếp whitelist, phân trang; projection sang `RecipeSummaryDto` (có URL thumbnail của ảnh chính); index `(Status, PublishedAt DESC)`, `CategoryId`, `CookTimeMinutes` | Test: Draft/đã xóa không xuất hiện; `sort` sai → 422; `pageSize` 100 → 422 | TV2 entity Recipe (27/09), TV3 RecipeImage (07/10 — trước đó để URL null) | T2 05/10 | M |
| 4.08 | Migration tìm kiếm: `CREATE EXTENSION unaccent, pg_trgm`, hàm `f_unaccent`, cột `SearchText`, GIN trigram index | Migration chạy lại được trên DB sạch; `EXPLAIN` cho thấy dùng index | TV2 migration Recipe (27/09) | T4 07/10 | M |
| 4.09 | Dữ liệu seed bằng Bogus: 50 recipe (nhiều trạng thái, tên món tiếng Việt thật), 5 tác giả, gán danh mục, vài recipe có ảnh mẫu | **Bàn giao cho nhóm:** lệnh seed chạy một lần ra dữ liệu demo và đo hiệu năng | TV1 user seed, TV3 Category + ảnh | T7 10/10 | M |
| 4.10 | `GET /recipes/search?q=`: chuẩn hóa từ khóa, `ILIKE` + trigram, sắp theo độ tương đồng, bộ lọc, chỉ Published; **bộ test tiếng Việt**: "phở bò" / "pho bo" / "Pho Bo" / "phơ bo" (gõ sai dấu) / "bò" chỉ 2 ký tự / "a" → 422 | Các test tiếng Việt xanh; từ khóa chứa `%`, `_`, `'` không gây lỗi | 4.08 | CN 11/10 | M |

### G2b — Frontend base (T2 12/10 → CN 18/10)

| Mã | Công việc | Kết quả bàn giao / tiêu chí xong | Phụ thuộc | Deadline | Mức |
|---|---|---|---|---|---|
| 4.11 | `/` trang chủ: khối recipe nổi bật, recipe mới nhất, lưới danh mục (API TV3); ISR `revalidate = 3600` | Trang hiển thị dữ liệu seed, không lỗi khi chưa có ảnh | 4.07, 4.09, TV3 `GET /categories` (03/10) | T3 13/10 | M |
| 4.12 | `/recipes`: bộ lọc (danh mục, độ khó, thời gian nấu, số khẩu phần), sắp xếp, phân trang — trạng thái đồng bộ với query string URL; skeleton khi tải | Chia sẻ URL giữ nguyên bộ lọc; nút quay lại trình duyệt hoạt động | 4.07 | T5 15/10 | M |
| 4.13 | `/search`: ô tìm kiếm ở header và trang kết quả, trạng thái rỗng có gợi ý, giữ từ khóa trên URL | Tìm "pho bo" ra "Phở bò" | 4.10 | CN 18/10 | M |

### G3 — Tích hợp & base nâng cao (T2 19/10 → CN 25/10)

| Mã | Công việc | Kết quả bàn giao / tiêu chí xong | Phụ thuộc | Deadline | Mức |
|---|---|---|---|---|---|
| 4.14 | Output Cache + Redis store: policy cho các GET công khai với TTL đã chốt; gắn tag; hiện thực `ICacheInvalidator` → `EvictByTagAsync` | **Bàn giao cho TV2, TV3:** sửa dữ liệu → response công khai cập nhật ngay; request có token không bị cache | TV2/TV3 đã gọi `ICacheInvalidator` | T4 21/10 | M |
| 4.15 | SEO base cho `/recipes/[slug]`, `/categories/[slug]`, `/`: `generateMetadata`, canonical, Open Graph/Twitter, JSON-LD `Recipe` (`name`, `description`, `image?`, `author`, `datePublished`, `prepTime`, `cookTime`, `totalTime`, `recipeYield`, `recipeIngredient`, `recipeInstructions`, `nutrition?`) | View source thấy đủ thẻ; JSON-LD hợp lệ theo schema.org | TV2 trang chi tiết (15/10) | T6 23/10 | M |
| 4.16 | `app/sitemap.ts` (recipe Published, danh mục, trang tĩnh; revalidate 1 giờ) + `app/robots.ts` khai báo sitemap | `/sitemap.xml` và `/robots.txt` truy cập được qua Nginx | 4.07 | T7 24/10 | M |
| 4.17 | Cài Playwright trong repo + E2E luồng tìm kiếm; viết mẫu hướng dẫn để TV1/TV2 viết E2E của họ | E2E chạy được bằng một lệnh | Luồng M3 xong | CN 25/10 | M |

### G4 — Kiểm thử, tài liệu, tổng duyệt (T2 26/10 → CN 01/11)

| Mã | Công việc | Kết quả bàn giao / tiêu chí xong | Phụ thuộc | Deadline | Mức |
|---|---|---|---|---|---|
| 4.18 | Đo NFR base: k6 (`GET /recipes`, `/recipes/{slug}`, `/recipes/search`, 50 VU, 5 phút); Lighthouse mobile trang chi tiết; Rich Results Test 3 recipe có ảnh | File `docs/bao-cao-do-nfr.md` có số đo, cấu hình máy, ảnh chụp kết quả | 4.14, 4.15, 4.09 | T4 28/10 | M |
| 4.19 | Tối ưu theo số đo (index, cache, kích thước ảnh, bundle JS) và đo lại nếu chưa đạt | Ghi kết quả trước/sau vào báo cáo | 4.18 | T5 29/10 | M |
| 4.20 | Gửi TV1 phần SRS v1.1: FR-RCP-001, 3.4 (viết lại theo trigram), 3.7 (OBS base, OTel chuyển mở rộng), Chương 4 (NFR base/mở rộng, sửa 99,5% ≈ 43,8 giờ/năm), 2.4.3 + 5.4 (bảng trình duyệt), 6.1–6.3 phần cache/log, 8.7 | Nội dung khớp thực tế | — | T5 29/10 | M |
| 4.21 | Tổng duyệt demo + trình bày số đo NFR | Kịch bản chạy trơn tru | — | T7 31/10 | M |
| 4.22 | Sửa lỗi cuối, gắn tag cùng nhóm | Tag `v1.0` | — | CN 01/11 | M |

## 5. Bàn giao và nhận

| Hướng | Nội dung | Với ai | Hạn |
|---|---|---|---|
| **Giao** | Repo, bảng Kanban, template PR | Cả nhóm | T6 18/09 |
| **Nhận** | Khung solution 4 tầng | TV2 | T4 23/09 |
| **Nhận** | Docker Compose (Seq, Redis) | TV3 | T5 24/09 |
| **Giao** | Khung Next.js + API client + proxy `/api` | TV1, TV2, TV3 | T6 25/09 |
| **Giao** | CI + script sinh kiểu TypeScript | Cả nhóm | CN 27/09 |
| **Nhận** | Entity Recipe + migration | TV2 | CN 27/09 |
| **Giao** | `ICacheInvalidator` (bản rỗng) | TV2, TV3 | T4 30/09 |
| **Nhận** | API danh mục | TV3 | T7 03/10 |
| **Giao** | Dữ liệu seed | Cả nhóm | T7 10/10 |
| **Nhận** | Trang `/recipes/[slug]` + DTO chi tiết | TV2 | T5 15/10 |
| **Giao** | Output Cache thật | TV2, TV3 | T4 21/10 |
| **Giao** | Playwright + mẫu E2E | TV1, TV2 | CN 25/10 |
| **Giao** | Phần SRS v1.1 + báo cáo đo NFR | TV1 | T5 29/10 |

## 6. Hướng phát triển sau 01/11

| Thứ tự | Hạng mục | Ghi chú |
|---|---|---|
| 1 | OpenTelemetry tracing (ASP.NET Core, HttpClient, EF Core) + metrics nghiệp vụ (recipe tạo/publish) → Seq qua OTLP | FR-OBS-003, đã chuyển sang mở rộng |
| 2 | Xếp hạng tốt hơn: kết hợp full-text `tsvector` (trọng số Title/Description) với trigram | Phương án C của S-08 |
| 3 | Tìm theo tên nguyên liệu; gợi ý từ khóa khi gõ | Cần cập nhật `SearchText` khi nguyên liệu thay đổi |
| 4 | Revalidate ISR theo sự kiện (backend gọi webhook `revalidateTag` khi publish) | Nội dung mới lên trang ngay |
| 5 | Lighthouse và k6 chạy tự động trong CI | NFR mở rộng |
| 6 | Dashboard giám sát (Seq signal/alert khi lỗi 5xx tăng, request chậm) | Hoàn thiện observability |
| 7 | Ảnh Open Graph riêng 1200×630 (phối hợp TV3) | Chia sẻ mạng xã hội đẹp hơn |

## 7. Rủi ro, dự phòng và checklist

**Rủi ro:**

| Rủi ro | Dấu hiệu | Dự phòng |
|---|---|---|
| Khung Next.js trễ làm cả nhóm không làm được frontend | T6 25/09 chưa bàn giao 4.03 | Bàn giao bản tối thiểu (Next.js + Tailwind + API client) trước, layout làm sau |
| Kết quả tìm kiếm không dấu nhiễu (ví dụ "pho" ra cả "phô mai") | Test 4.10 cho thứ tự kém | Tăng ngưỡng tương đồng; ưu tiên khớp đầu từ trong Title; hạn chế độ dài Description đưa vào `SearchText` |
| Output Cache làm dữ liệu cũ khó phát hiện | Sửa bài không thấy cập nhật | Kiểm tra tag trong 4.14; có biến cấu hình tắt cache nhanh khi demo |
| Số đo hiệu năng không đạt | 4.18 p95 > 500 ms | Kiểm tra index + cache trước; nếu vẫn chưa đạt thì ghi rõ nguyên nhân và hướng xử lý trong báo cáo (NFR base cho phép ghi nhận) |
| Quá nhiều việc "nền" (repo, CI, Next.js, log, seed) | CN 27/09 chưa xong 4.04 | Nhờ TV3 hỗ trợ phần GitHub Actions cho .NET |

**Checklist tự kiểm tra trước CN 01/11:**
- [ ] CI chạy trên mọi PR; kiểu TypeScript sinh từ OpenAPI
- [ ] Log trong Seq có `CorrelationId`, `UserId`; không có mật khẩu/token
- [ ] `/health/ready` chỉ phụ thuộc DB
- [ ] Danh sách và tìm kiếm chỉ trả Published, chưa xóa
- [ ] "pho bo", "Phở Bò", "phơ bo" đều tìm ra "Phở bò"
- [ ] Tham số lọc/sắp xếp sai → 422
- [ ] Response công khai được cache; sửa dữ liệu thì cache bị xóa đúng tag
- [ ] Trang chi tiết có metadata, Open Graph, JSON-LD hợp lệ; `sitemap.xml` và `robots.txt` hoạt động
- [ ] Có báo cáo đo k6 + Lighthouse + Rich Results
- [ ] SRS v1.1 phần 3.4, 3.7, Chương 4, 8.7 khớp thực tế
