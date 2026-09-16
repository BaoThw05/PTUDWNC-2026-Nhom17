# Kế hoạch cá nhân — TV2 · Bùi Ngọc Toàn

> **Module:** Công thức nấu ăn cốt lõi (FR-RCP-002 → 007, FR-RCP-009, FR-RCP-010)
> **Vai trò chung:** nền backend — khung solution 4 tầng, BaseEntity, xử lý lỗi chung, hợp đồng API
> **Hạn chốt:** CN 01/11/2026 — ứng dụng hoàn thiện (đăng nhập, giao diện đầy đủ, web gần như hoàn chỉnh) · Xem mốc chung trong `00_KeHoach_TongThe.md`

## 1. Phạm vi trách nhiệm

| Nhóm | Nội dung |
|---|---|
| Nền backend | Solution Domain/Application/Infrastructure/API + test project + architecture test; `BaseEntity`, `AppDbContext`, `AuditInterceptor`, `IUnitOfWork`; MediatR + `ValidationBehavior`; `IExceptionHandler` + Problem Details + bảng mã lỗi; `PagedResult<T>` |
| Backend module | Entity Recipe/RecipeStep/RecipeIngredient/RecipeNutrition; tạo, sửa (concurrency), xem theo id và theo slug, "bài của tôi"; máy trạng thái publish/unpublish/archive/unarchive; steps; ingredients; xóa mềm vào thùng rác, khôi phục, job dọn thùng rác |
| Frontend | `/dashboard/recipes`, `/dashboard/recipes/new` (wizard), `/dashboard/recipes/[id]/edit`, `/recipes/[slug]` |
| Lỗi SRS phụ trách xử lý | B-01, B-02, B-03, B-04, B-05, B-08, C-07, C-09, C-11, D-01, D-03, D-10 (phần nền), D-20, D-21, D-28, D-29, G-03, G-06; nền chung: C-04, C-05, C-06, B-09 |

## 2. Quyết định áp dụng cho phần việc

- **Xóa dữ liệu (S-03 — nhóm đã chốt phương án C):**
  - `Recipe` có `IsDeleted` + `DeletedAt`; Global Query Filter chỉ áp dụng cho Recipe. Bảng con (steps, ingredients, images) và Category **xóa thật**.
  - `DELETE /recipes/{id}` → vào thùng rác (ẩn khỏi mọi trang công khai, xóa cache). `POST /recipes/{id}/restore` → khôi phục về trạng thái trước khi xóa; slug đã bị bài khác dùng thì thêm hậu tố.
  - Job định kỳ xóa thật recipe nằm trong thùng rác **quá 30 ngày**, kèm xóa ảnh theo prefix storage.
  - Slug: partial unique index `WHERE "IsDeleted" = false`.
- **Concurrency (S-04):** `uint Version` ánh xạ cột `xmin`; `GET` trả `version`; `PUT` bắt buộc gửi `version`; lệch → 409 `RECIPE_CONCURRENCY_CONFLICT`. Mọi thao tác trên steps/ingredients/images cập nhật `Recipe.UpdatedAt` để `xmin` đổi.
- **Vòng đời (S-11):** Draft → Published (điều kiện: **≥ 1 bước**; nguyên liệu và ảnh **không bắt buộc**, được bổ sung sau) · Published → Draft · Draft/Published → Archived · Archived → Draft. `PublishedAt` set ở lần publish đầu tiên và giữ nguyên. Trùng trạng thái đích → 200 (idempotent). Xóa bước cuối cùng của recipe Published → 422 `RECIPE_PUBLISH_INCOMPLETE`.
- **Hiển thị (S-11):** endpoint công khai chỉ trả Published và chưa xóa; Draft của người khác → 404. Dữ liệu cá nhân đi qua `GET /me/recipes` và `GET /recipes/{id:guid}`.
- **Slug (S-11):** tự thêm hậu tố `-2`, `-3`…; sinh lại khi đổi tiêu đề **chỉ khi chưa từng publish**; hàm slugify tiếng Việt đổi `đ/Đ → d`; `search` là slug cấm.
- **Validation (bảng B-08):** Title 5–200; Description 1–2000; `PrepTimeMinutes > 0`; `CookTimeMinutes ≥ 0`; `Servings > 0`; Difficulty Easy/Medium/Hard; Nutrition 6 trường, đều không bắt buộc; nguyên liệu Name 1–200, Quantity/Unit được null (khi có thì Quantity > 0); bước có Title không bắt buộc (≤ 200), Description 1–2000, `DurationMinutes ≥ 0`. Nội dung là **plain text**; bỏ trường `Instructions`.
- **Kiến trúc (S-13):** Domain không NuGet; `IUnitOfWork` và interface repository ở Application; pipeline MediatR: Logging (TV4) → Validation → Handler.
- **Hợp đồng API (S-10):** không vỏ bọc; 422 cho validation và vi phạm quy tắc nghiệp vụ; Problem Details có `code`, `traceId`, `errors`; `PagedResult<T> { items, page, pageSize, totalCount, totalPages, hasNextPage, hasPreviousPage }`; `sort=-createdAt`; tên JSON trùng tên thuộc tính C#.
- **Mã lỗi:** `RECIPE_NOT_FOUND` (404), `RECIPE_FORBIDDEN` (403), `RECIPE_CONCURRENCY_CONFLICT` (409), `RECIPE_PUBLISH_INCOMPLETE` (422), `RECIPE_CATEGORY_INVALID` (422), `RECIPE_INVALID_STATE_TRANSITION` (422), `STEP_NOT_FOUND`, `INGREDIENT_NOT_FOUND` (404).

## 3. Mốc cá nhân

| Ngày | Mốc cá nhân | Gắn với mốc chung |
|---|---|---|
| CN 20/09 | ADR xóa dữ liệu, concurrency, hợp đồng API, vòng đời, kiến trúc | M0 |
| T4 23/09 | Khung solution build được | — |
| CN 27/09 | Entity Recipe + migration cho cả nhóm | — |
| CN 11/10 | Toàn bộ API recipe | M2 |
| CN 18/10 | Dashboard, wizard, trang sửa, trang chi tiết | M3 |
| CN 25/10 | Cache tag, job dọn thùng rác | M4 |

## 4. Công việc chi tiết và deadline

**Mức ưu tiên:** M = Must · S = Should (trễ thì chuyển sau 01/11).

### G0 — Chốt quyết định (T5 17/09 → CN 20/09)

| Mã | Công việc | Kết quả bàn giao / tiêu chí xong | Phụ thuộc | Deadline | Mức |
|---|---|---|---|---|---|
| 2.01 | ADR S-03 (xóa lai + thùng rác 30 ngày), S-04 (concurrency), S-11 (vòng đời, hiển thị, slug) | 3 file ADR ngắn trong `docs/adr/` | TV1 tạo mẫu ADR | CN 20/09 | M |
| 2.02 | ADR S-10 (hợp đồng API) + **bảng mã lỗi chung** + bảng đặt tên; ADR S-13 (ranh giới kiến trúc, phiên bản/giấy phép MediatR) | Bảng mã lỗi có cột Mã / HTTP / Mô tả / Module để các thành viên tự thêm mã | — | CN 20/09 | M |

### G1 — Nền tảng (T2 21/09 → T4 30/09)

| Mã | Công việc | Kết quả bàn giao / tiêu chí xong | Phụ thuộc | Deadline | Mức |
|---|---|---|---|---|---|
| 2.03 | Solution 4 project + tham chiếu đúng chiều; `Directory.Build.props` (.NET 10, nullable, analyzer mặc định); project unit test + integration test; architecture test (NetArchTest/ArchUnitNET) | **Bàn giao cho nhóm:** `dotnet build` và `dotnet test` chạy; architecture test fail nếu Domain tham chiếu gói ngoài | — | T4 23/09 | M |
| 2.04 | `BaseEntity` (`Guid Id`, `CreatedAt`, `UpdatedAt`, `uint Version` = `xmin`); `AppDbContext` (Npgsql); `AuditInterceptor`; `IUnitOfWork`; interface `ISoftDeletable` + Global Query Filter | **Bàn giao cho TV1, TV3:** `AppDbContext` + `BaseEntity` dùng được; test: sửa cùng một dòng từ 2 context → `DbUpdateConcurrencyException` | 2.03, TV3 compose (24/09) | T6 25/09 | M |
| 2.05 | MediatR + `ValidationBehavior` + đăng ký FluentValidation; `IExceptionHandler` + `AddProblemDetails` (map exception → status + `code`); `PagedResult<T>`; quy ước nhóm endpoint | **Bàn giao cho nhóm:** ném `ValidationException` → 422 có `errors`; `NotFoundException` → 404 có `code` | 2.03 | T7 26/09 | M |
| 2.06 | Domain Recipe, RecipeStep, RecipeIngredient, RecipeNutrition (owned); enum Difficulty, Status; `IsDeleted`/`DeletedAt`; partial unique index Slug; index `AuthorId`, `CategoryId`; migration | **Bàn giao cho TV3, TV4:** bảng `Recipes` có sẵn trong DB | 2.04, TV3 entity Category (26/09) | CN 27/09 | M |
| 2.07 | Endpoint *stub* cho `/recipes/*` (dữ liệu giả) để OpenAPI có schema sớm | Frontend sinh được kiểu TypeScript cho recipe | 2.06, TV4 script sinh kiểu (27/09) | T4 30/09 | M |

### G2a — Backend base (T5 01/10 → CN 11/10)

| Mã | Công việc | Kết quả bàn giao / tiêu chí xong | Phụ thuộc | Deadline | Mức |
|---|---|---|---|---|---|
| 2.08 | `POST /recipes`: tạo Draft; slugify tiếng Việt + hậu tố; validation; steps/ingredients gửi kèm tùy chọn; category không tồn tại → 422 | Test: tạo thành công, trùng tiêu đề sinh `-2`, dữ liệu sai → 422, chưa đăng nhập → 401 | TV1 JWT (29/09) | T6 02/10 | M |
| 2.09 | `GET /recipes/{id:guid}` (chủ bài/Admin, mọi trạng thái) + `GET /me/recipes?status=&page=&pageSize=` (có lọc `deleted` cho thùng rác) | Test: người khác xem bài của mình → 404; lọc đúng theo trạng thái | 2.08 | CN 04/10 | M |
| 2.10 | `PUT /recipes/{id}`: bắt buộc `version`; `RecipeAuthorizationHandler` (chủ bài hoặc Admin); slug sinh lại khi `PublishedAt` null | Test: version lệch → 409; người khác sửa → 403; Admin sửa được | 2.09 | T3 06/10 | M |
| 2.11 | Máy trạng thái: `PATCH /recipes/{id}/publish`, `/unpublish`, `/archive`, `/unarchive`; điều kiện ≥ 1 bước; `PublishedAt`; idempotent | Test: publish không có bước → 422; chuyển trạng thái sai → 422; gọi 2 lần → 200 | 2.10 | T4 07/10 | M |
| 2.12 | `GET /recipes/{slug}` công khai: chỉ Published + chưa xóa; projection hoặc `AsSplitQuery()` (steps, ingredients, images, nutrition, category, tác giả) | Test: Draft → 404; chỉ sinh ra số câu SQL cố định (xem log EF) | 2.11 | T5 08/10 | M |
| 2.13 | Steps: `POST` (`StepNumber` tự sinh, khóa dòng Recipe khi thêm), `PUT` (không đổi số), `DELETE` (đánh lại số theo cách tạm +1000); chặn xóa bước cuối của recipe Published; cập nhật `Recipe.UpdatedAt` | Test: xóa bước giữa → số liên tục; thêm đồng thời 2 bước không trùng số; xóa bước cuối khi Published → 422 | 2.11 | T6 09/10 | M |
| 2.14 | Ingredients: `POST`, `PUT`, `DELETE`; `OrderIndex`; cập nhật `Recipe.UpdatedAt` | Test: thêm/sửa/xóa; Quantity null hợp lệ | 2.11 | T7 10/10 | M |
| 2.15 | Thùng rác: `DELETE /recipes/{id}` (xóa mềm, gọi `ICacheInvalidator`), `POST /recipes/{id}/restore`; ghi `RecipeDeleted`/`RecipeRestored` vào log | Test: xóa → biến mất khỏi danh sách công khai và tìm kiếm; khôi phục → quay lại đúng trạng thái; slug trùng khi khôi phục → có hậu tố | 2.12, TV4 `ICacheInvalidator` (30/09) | CN 11/10 | M |

### G2b — Frontend base (T2 12/10 → CN 18/10)

| Mã | Công việc | Kết quả bàn giao / tiêu chí xong | Phụ thuộc | Deadline | Mức |
|---|---|---|---|---|---|
| 2.16 | `/dashboard/recipes`: bảng "bài của tôi", tab lọc Draft / Published / Archived / Thùng rác; nút publish, unpublish, archive, unarchive, xóa, khôi phục (có hộp xác nhận) | Mọi thao tác cập nhật danh sách ngay (TanStack Query invalidate) | TV1 Auth.js (02/10) | T3 13/10 | M |
| 2.17 | `/recipes/[slug]` trang chi tiết công khai: ảnh, thông tin, dinh dưỡng, nguyên liệu, các bước; ISR `revalidate = 300`; chừa vị trí cho metadata/JSON-LD | **Bàn giao cho TV4:** DTO chi tiết và route ổn định | 2.12 | T5 15/10 | M |
| 2.18 | `/dashboard/recipes/new` wizard: B1 thông tin + dinh dưỡng → B2 nguyên liệu → B3 các bước → B4 ảnh (component của TV3) → Lưu nháp / Publish | Tạo được recipe đầy đủ trên giao diện; lỗi 422 hiển thị đúng ô | 2.08, 2.13, 2.14, TV3 component ảnh (15/10) | T7 17/10 | M |
| 2.19 | `/dashboard/recipes/[id]/edit`: tải theo id, sửa từng phần; nhận 409 → báo "dữ liệu đã bị thay đổi", cho tải lại | Mở 2 tab sửa cùng bài → tab lưu sau thấy thông báo 409 | 2.10, 2.18 | CN 18/10 | M |

### G3 — Tích hợp & base nâng cao (T2 19/10 → CN 25/10)

| Mã | Công việc | Kết quả bàn giao / tiêu chí xong | Phụ thuộc | Deadline | Mức |
|---|---|---|---|---|---|
| 2.20 | Rà lại mọi command của recipe/steps/ingredients gọi `ICacheInvalidator` đúng tag (`recipes`, `recipe:{id}`, `categories` khi publish/unpublish/archive/xóa) sau khi TV4 bật cache thật | Sửa bài → trang công khai cập nhật ngay, không chờ hết TTL | TV4 Output Cache (21/10) | T5 22/10 | M |
| 2.21 | Recurring job dọn thùng rác: recipe xóa mềm quá 30 ngày → xóa thật (cascade) + `DeleteByPrefix("recipes/{id}/")` | Test với `DeletedAt` giả lập quá 30 ngày; job hiện trong Hangfire | TV3 `IFileStorage` (27/09), Hangfire (29/09) | T6 23/10 | M |
| 2.22 | `PUT /recipes/{id}/steps/order` (nhận danh sách id) + kéo thả sắp xếp bước trong wizard/trang sửa | Kéo thả xong tải lại vẫn đúng thứ tự | 2.13, 2.19 | CN 25/10 | S |

### G4 — Kiểm thử, tài liệu, tổng duyệt (T2 26/10 → CN 01/11)

| Mã | Công việc | Kết quả bàn giao / tiêu chí xong | Phụ thuộc | Deadline | Mức |
|---|---|---|---|---|---|
| 2.23 | Hoàn thiện integration test cho mọi endpoint recipe (thành công, 401/403/404/409/422); E2E Playwright luồng tạo recipe → publish → xem trang chi tiết | CI xanh | TV4 Playwright (25/10) | T4 28/10 | M |
| 2.24 | Architecture test xanh; build không warning; rà N+1 trên trang chi tiết và "bài của tôi" | Không còn truy vấn lặp theo từng dòng | — | T5 29/10 | M |
| 2.25 | Gửi TV1 phần SRS v1.1: 3.3 (FR-RCP-002 → 007, 009, 010 viết lại theo quyết định), 7.1–7.4, 8.3, 8.5, 8.6, mã `RECIPE_*` ở Phụ lục B | Nội dung khớp OpenAPI thực tế | — | T5 29/10 | M |
| 2.26 | Tổng duyệt demo (tạo, sửa, publish, xóa vào thùng rác, khôi phục) | Kịch bản chạy trơn tru | — | T7 31/10 | M |
| 2.27 | Sửa lỗi cuối, gắn tag cùng nhóm | Tag `v1.0` | — | CN 01/11 | M |

## 5. Bàn giao và nhận

| Hướng | Nội dung | Với ai | Hạn |
|---|---|---|---|
| **Giao** | Bảng mã lỗi chung + bảng đặt tên | Cả nhóm | CN 20/09 |
| **Giao** | Khung solution 4 tầng | TV1, TV3, TV4 | T4 23/09 |
| **Nhận** | Docker Compose | TV3 | T5 24/09 |
| **Giao** | `AppDbContext` + `BaseEntity` | TV1, TV3 | T6 25/09 |
| **Giao** | Problem Details + `PagedResult` + `ValidationBehavior` | Cả nhóm | T7 26/09 |
| **Nhận** | Entity Category | TV3 | T7 26/09 |
| **Giao** | Entity Recipe + migration | TV3, TV4 | CN 27/09 |
| **Nhận** | `IFileStorage` (có `DeleteByPrefix`) | TV3 | CN 27/09 |
| **Nhận** | JWT + tài khoản seed | TV1 | T3 29/09 |
| **Nhận** | Hangfire + `IBackgroundJobService` | TV3 | T3 29/09 |
| **Nhận** | `ICacheInvalidator` bản rỗng | TV4 | T4 30/09 |
| **Nhận** | Auth.js session + middleware | TV1 | T6 02/10 |
| **Giao** | Trang `/recipes/[slug]` + DTO chi tiết | TV4 | T5 15/10 |
| **Nhận** | Component upload ảnh | TV3 | T5 15/10 |
| **Nhận** | Output Cache thật | TV4 | T4 21/10 |
| **Giao** | Phần SRS v1.1 | TV1 | T5 29/10 |

## 6. Hướng phát triển sau 01/11

| Thứ tự | Hạng mục | Ghi chú |
|---|---|---|
| 1 | Kéo thả sắp xếp bước (nếu 2.22 bị trễ) | Endpoint đã thiết kế |
| 2 | Sắp xếp lại nguyên liệu bằng kéo thả | Tương tự 2.22 |
| 3 | Danh sách toàn bộ recipe cho Admin (`GET /admin/recipes`), lọc theo tác giả/trạng thái | Phục vụ kiểm duyệt |
| 4 | Nội dung dạng Markdown cho các bước | Renderer không cho HTML thô |
| 5 | Xem trước (preview) trước khi publish; nhân bản recipe | Cải thiện trải nghiệm tác giả |
| 6 | Lịch sử chỉnh sửa (lưu phiên bản) | Tận dụng `Version`/`UpdatedAt` |
| 7 | Chuyển recipe hàng loạt sang danh mục khác (phối hợp TV3) | Giải quyết B-07 triệt để |

## 7. Rủi ro, dự phòng và checklist

**Rủi ro:**

| Rủi ro | Dấu hiệu | Dự phòng |
|---|---|---|
| Khối lượng G2a lớn (8 nhóm endpoint trong 11 ngày) | CN 04/10 chưa xong 2.08, 2.09 | Nhờ TV4 (G2a nhẹ hơn) nhận 2.14 ingredients; giữ nguyên hợp đồng API đã chốt |
| Đánh số lại bước vi phạm unique | Test 2.13 lỗi `23505` | Khai báo unique constraint `DEFERRABLE INITIALLY DEFERRED` bằng SQL migration |
| `xmin` không bắt được xung đột | Test 2.04 không ném exception | Kiểm tra cấu hình `IsRowVersion()`; kiểm tra original value được gán từ `version` client gửi |
| Wizard phụ thuộc component ảnh | TV3 trễ 3.13 quá T5 15/10 | Wizard tạm bỏ bước ảnh, cho thêm ảnh ở trang sửa (ảnh không bắt buộc để publish) |
| Nhiệm vụ nền lấn thời gian module | T4 30/09 chưa xong 2.07 | Bỏ endpoint stub, frontend dùng trực tiếp API thật từ 02/10 |

**Checklist tự kiểm tra trước CN 01/11:**
- [ ] Architecture test xanh; Domain không có gói NuGet
- [ ] Mọi lỗi trả Problem Details có `code`; validation → 422
- [ ] Sửa đồng thời → 409; sửa bảng con làm đổi `version` của Recipe
- [ ] Publish thiếu bước → 422; nguyên liệu và ảnh không bắt buộc
- [ ] Draft/Archived/đã xóa không xuất hiện ở endpoint công khai
- [ ] Xóa → vào thùng rác; khôi phục được; job dọn quá 30 ngày chạy trong Hangfire
- [ ] Slug tiếng Việt đúng (`Phở bò` → `pho-bo`, `Đậu hũ` → `dau-hu`), trùng thì có hậu tố
- [ ] Không có truy vấn N+1 ở trang chi tiết
- [ ] SRS v1.1 phần 3.3, 7.x, 8.3/8.5/8.6 khớp OpenAPI
