# Danh sách lỗi & mâu thuẫn — SRS Culinary Blog v1.0.0

> **Tài liệu được rà soát:** `SRS_Culinary_Blog_v1.0.0.pdf` (71 trang, bản "Approved" 04/06/2026) — bản Markdown đối chiếu: `SRS_Culinary_Blog_v1.0.0.md`.
> **Ngày rà soát:** 16/09/2026. **Phương pháp:** đọc toàn văn; đối chiếu chéo Chương 1–8 và Phụ lục A–C (FR ↔ NFR ↔ UI ↔ kiến trúc ↔ mô hình dữ liệu ↔ API); kiểm tra tính khả thi kỹ thuật với .NET 10 / EF Core + Npgsql / PostgreSQL 16 / Next.js; kiểm tra các thông tin có thể đã lỗi thời tại thời điểm 09/2026 (nguồn ở cuối tài liệu).
> **Phạm vi:** chỉ **liệt kê và chứng minh** lỗi. Phương án xử lý (ưu/nhược điểm, khuyến nghị, hướng đi) nằm trong file `SRS_Culinary_Blog_v1.0.0_GiaiPhap.md`.

## Quy ước

**Mức độ**

| Mức | Ý nghĩa |
|---|---|
| **Nghiêm trọng** | Nếu làm đúng như SRS viết thì hệ thống sai chức năng, có lỗ hổng bảo mật, hoặc không chạy được. Phải chốt trước khi code phần liên quan. |
| **Cao** | Các chương nói khác nhau → mỗi thành viên sẽ hiện thực một kiểu, vỡ khi tích hợp FE/BE. |
| **Trung bình** | Thiếu sót, mơ hồ, rủi ro vận hành; có thể sửa trong lúc làm nhưng cần ghi nhận. |
| **Thấp** | Lỗi biên tập, trình bày, thuật ngữ, trích dẫn cũ. |

**Vị trí** ghi theo mục trong SRS và số trang PDF (tr.). **Phụ trách** theo `BangPhanCong.docx`: TV1 = Auth & Profile + FR-JOB-001; TV2 = Recipe Core; TV3 = Categories + File/MinIO + Image; TV4 = Search/SEO/Observability; **Chung** = phần nền tảng cả nhóm.

**Nhóm lỗi**

- **A** — Lỗi cấu trúc & biên tập tài liệu
- **B** — Mâu thuẫn nghiệp vụ & mô hình dữ liệu
- **C** — Mâu thuẫn hợp đồng API (Chương 3 ↔ 5 ↔ 8 ↔ Phụ lục)
- **D** — Sai hoặc không khả thi về kỹ thuật
- **E** — NFR sai số liệu / không đo được / không thực tế
- **F** — Công nghệ lỗi thời hoặc có rủi ro tại thời điểm 09/2026
- **G** — Thiếu sót yêu cầu

---

## Tóm tắt

| Nhóm | Nghiêm trọng | Cao | Trung bình | Thấp | Tổng |
|---|---|---|---|---|---|
| **A** — Cấu trúc & biên tập | 0 | 1 | 3 | 4 | 8 |
| **B** — Nghiệp vụ & dữ liệu | 1 | 7 | 3 | 0 | 11 |
| **C** — Hợp đồng API | 2 | 7 | 1 | 1 | 11 |
| **D** — Kỹ thuật | 5 | 14 | 9 | 4 | 32 |
| **E** — NFR | 0 | 1 | 3 | 2 | 6 |
| **F** — Công nghệ lỗi thời | 1 | 3 | 1 | 1 | 6 |
| **G** — Thiếu sót | 0 | 1 | 6 | 1 | 8 |
| **Tổng** | **9** | **34** | **26** | **13** | **82** |

**Các lỗi Nghiêm trọng (cần chốt trước khi viết code):**

| Mã | Vấn đề |
|---|---|
| B-01 | Xóa công thức/danh mục: hard delete hay soft delete? |
| C-01 | Hợp đồng đăng ký tài khoản khác nhau hoàn toàn |
| C-03 | Đăng nhập Google được mô tả bằng 3 luồng khác nhau |
| D-01 | `RowVersion bytea [Timestamp]` không hoạt động trên PostgreSQL |
| D-02 | Full-text search tiếng Việt viết sai về PostgreSQL |
| D-03 | Cache response nhưng nội dung lại phụ thuộc người xem |
| D-04 | Bốn cơ chế cache với TTL mâu thuẫn nhau |
| D-05 | Lỗ hổng trong luồng đăng nhập Google |
| F-01 | MinIO bản cộng đồng đã ngừng phát hành |

**Nguyên nhân gốc:** (1) SRS trộn lẫn *yêu cầu* với *thiết kế chi tiết* (tên class, lời gọi hàm, TTL cụ thể), nên cùng một quyết định được viết lại ở 3–5 chỗ và mỗi chỗ một khác; (2) các chương được viết ở các thời điểm khác nhau (Ch.3 ở v0.8, Ch.7–8 ở v0.9) mà không đối chiếu lại; (3) một số thông tin công nghệ đã lỗi thời trước hoặc ngay sau ngày phát hành.

---

## A. Lỗi cấu trúc & biên tập tài liệu

### A-01 · Cao · Số lượng FR mâu thuẫn: "27 FR" nhưng thực tế là 34
- **Vị trí:** 1.5 (tr. 10); 2.2 (tr. 12); mở đầu Chương 3 (tr. 17).
- **Nội dung:** cả ba chỗ đều viết "27 Functional Requirements". Chính bảng 2.2 liệt kê 7 (AUTH) + 5 (CAT) + 10 (RCP) + 4 (SRCH) + 2 (FILE) + 3 (JOB) + 3 (OBS) = **34**, và Chương 3 cũng có đúng 34 mã FR.
- **Hệ quả:** không xác định được baseline để lập test case và ma trận truy vết; ai đếm theo "27" sẽ bỏ sót 7 FR.
- **Phụ trách:** Chung.

### A-02 · Trung bình · 11 FR không theo template đã cam kết
- **Vị trí:** mở đầu Chương 3 (tr. 17) cam kết mọi FR theo template gồm Tác nhân, Mức ưu tiên (MoSCoW), Điều kiện tiên quyết, Luồng chính, Luồng thay thế/Ngoại lệ, HTTP Endpoint, Kết quả mong đợi, HTTP Status Code; FR-SRCH-002/003/004 (tr. 37), FR-FILE-001/002 (tr. 37–38), FR-JOB-001/002/003 (tr. 38–39), FR-OBS-001/002/003 (tr. 39).
- **Nội dung:** 11 FR trên chỉ là một dòng trong bảng tóm tắt: **không có mức ưu tiên MoSCoW**, không có luồng ngoại lệ, không có tiêu chí chấp nhận.
- **Hệ quả:** không biết FR nào là Must/Should; không viết được test chấp nhận (ví dụ: FR-JOB-002 lỗi thì trả gì cho người dùng?).
- **Phụ trách:** TV1 (JOB-001), TV3 (FILE, JOB-002), TV4 (SRCH, JOB-003, OBS).

### A-03 · Thấp · Bảng tổng hợp NFR sai số lượng và mô tả sai mô hình chất lượng
- **Vị trí:** mở đầu Chương 4 (tr. 40).
- **Nội dung:** (1) NFR-SEC ghi "6" yêu cầu nhưng thực tế có 7 (NFR-SEC-001 → 007). (2) Tuyên bố "mỗi NFR được gán mức ưu tiên" nhưng từng NFR không có ưu tiên (chỉ có ưu tiên theo nhóm). (3) Viết "ISO/IEC 25010 (FURPS+)" như thể là một — thực tế ISO/IEC 25010 và FURPS+ (của HP) là hai mô hình khác nhau.
- **Phụ trách:** Chung.

### A-04 · Thấp · Mục 1.5 mô tả sai nội dung các chương
- **Vị trí:** 1.5 (tr. 10).
- **Nội dung:** (1) "Chương 5 … SendGrid" nhưng 5.3 dùng SMTP/MailKit, không có SendGrid. (2) "Chương 6 … Next.js App Router Frontend, chiến lược caching" nhưng Chương 6 không có mục kiến trúc frontend hay chiến lược cache. (3) "Chương 7 … ERD mô tả văn bản" nhưng ERD nằm ở 6.4 và không có sơ đồ.
- **Phụ trách:** Chung.

### A-05 · Thấp · Đánh số chương, header/footer không thống nhất
- **Vị trí:** Mục lục (tr. 3–5), toàn tài liệu.
- **Nội dung:** Chương 1–3 đặt tên "CHƯƠNG N." nhưng 4–8 chỉ là "4.", "5."…; footer tr. 2–5 là "Phát triển Ứng dụng Web Nâng cao", từ tr. 6 thành "… Nâng cao V4"; mục 7.7 bị dàn trang như đoạn văn thường; footer "CONFIDENTIAL" trên tài liệu giáo trình phát cho sinh viên.
- **Phụ trách:** Chung.

### A-06 · Trung bình · Trạng thái "Approved" nhưng còn nhiều yêu cầu chưa quyết định
- **Vị trí:** Lịch sử thay đổi (tr. 2) và rải rác.
- **Nội dung:** ISO/IEC/IEEE 29148 yêu cầu mỗi yêu cầu phải rõ ràng và kiểm chứng được, nhưng bản "Approved" vẫn còn nhiều lựa chọn bỏ ngỏ: "Upload sitemap.xml lên MinIO **hoặc** lưu vào wwwroot" (FR-JOB-003); "Auth.js v5 (frontend) **hoặc** ASP.NET Google provider" (6.2); "KHÔNG cache (…) **hoặc** cache ngắn (5 phút)" (FR-SRCH-001); "**Có thể** kích hoạt revoke toàn bộ refresh tokens của user đó (paranoid mode)" (FR-AUTH-004); "Presigned URL … (optional)" (5.3); "Cookie … (**nếu** dùng cookie cho refresh token)" (NFR-SEC-005); "localStorage/cookie" (FR-AUTH-005). Ngoài ra lịch sử phiên bản không hề nhắc tới lúc Chương 5 và 6 được viết.
- **Phụ trách:** Chung.

### A-07 · Trung bình · PDF bị mất nội dung do bảng tràn lề
- **Vị trí:** FR-RCP-001, FR-RCP-002 (tr. 28–29); 5.3 cột "Cấu hình / Secrets" (tr. 47–48); 8.2–8.3 (tr. 62–64).
- **Nội dung:** chữ ở mép phải bị cắt mất ngay trong file PDF (không phải lỗi chuyển đổi): mất phạm vi `pageSize`, giá trị `sort=`, tên cache policy, tên biến môi trường Google/MinIO/Hangfire/Seq/OTel, request body của POST/PUT `/recipes`. Ở tr. 63, tiêu đề "8.3" bị chèn vào giữa bảng 8.2, và **bảng 8.3 không có cột Response**.
- **Hệ quả:** một số yêu cầu không thể đọc được. Nếu còn file Word gốc thì cần xuất lại PDF.
- **Phụ trách:** Chung.

### A-08 · Thấp · Hai bảng thuật ngữ chồng nhau, có mục thừa hoặc lỗi thời
- **Vị trí:** 1.3 (tr. 7–8); Phụ lục C (tr. 69–71).
- **Nội dung:** thuật ngữ trùng lặp giữa hai bảng; Phụ lục C có "CQRS" hai lần; 1.3 có "DXA — Device-independent pixel unit used in OOXML" (đơn vị dàn trang Word, không liên quan, chưa dịch — sót lại từ template); "HTTP Status Code (RFC 7231)" — RFC 7231 đã bị RFC 9110 thay thế (2022).
- **Phụ trách:** Chung.

---

## B. Mâu thuẫn nghiệp vụ & mô hình dữ liệu

### B-01 · Nghiêm trọng · Xóa công thức/danh mục: hard delete hay soft delete?
- **Vị trí:** FR-RCP-007 (tr. 32–33); FR-CAT-005 (tr. 26–27); NFR-REL-003 (tr. 43); 6.4 (tr. 52); mở đầu Chương 7 và 7.1 (tr. 54); 8.2, 8.3 (tr. 63–64); Phụ lục A dòng 404 (tr. 67); Phụ lục C "Soft Delete" (tr. 71).
- **Nội dung:**
  - FR-RCP-007: "**Xóa vĩnh viễn** … cascade delete … Đây là **hard delete** (không dùng soft delete pattern cho recipe)", đồng thời xóa luôn file ảnh trên MinIO.
  - NFR-REL-003: "Soft delete: **Recipe được đánh dấu IsDeleted** thay vì xóa vật lý (có thể khôi phục)".
  - Chương 7: "Tất cả entities kế thừa BaseEntity và **sử dụng Soft Delete pattern**"; 8.3: `DELETE /recipes/{id}` "Xóa recipe **(soft delete)**".
  - FR-CAT-005: "Danh mục **bị xóa khỏi database**" ↔ 8.2: `DELETE /categories/{id}` "**(soft delete)**".
- **Hệ quả nếu trộn cả hai:** `ON DELETE CASCADE` không chạy khi soft delete → Steps/Ingredients/Images vẫn còn; ảnh MinIO đã bị xóa trong khi bản ghi "có thể khôi phục"; ràng buộc `UNIQUE` của `Slug`, `Category.Name`, `(RecipeId, StepNumber)` bị các bản ghi đã xóa mềm chiếm chỗ (xem D-21); FR-CAT-005 đếm recipe qua Global Query Filter sẽ bỏ qua bản ghi xóa mềm, nhưng FK `RESTRICT` vẫn thấy chúng → lỗi 500 khi xóa danh mục.
- **Phụ trách:** Chung, TV2, TV3.

### B-02 · Cao · Điều kiện xuất bản (publish) công thức có 3 phiên bản
- **Vị trí:** FR-RCP-005 (tr. 31–32); Phụ lục A dòng 400 (tr. 67); Phụ lục B `RECIPE_PUBLISH_INCOMPLETE` (tr. 68); NFR-SEO-001 (tr. 44); FR-RCP-009/010 (tr. 34–36).
- **Nội dung:**
  - FR-RCP-005: chỉ cần "ít nhất 1 bước thực hiện", lỗi trả **422**.
  - Phụ lục A: "publish recipe **thiếu ingredients**" trả **400**.
  - Phụ lục B: "phải có ít nhất **1 ingredient và 1 step**", trả **400**.
  - NFR-SEO-001: mọi trang công thức "phải pass 100%" Google Rich Results Test, nhưng Google yêu cầu bắt buộc thuộc tính `image` cho Recipe — trong khi publish không đòi hỏi có ảnh.
  - Không FR nào chặn việc xóa bước/nguyên liệu cuối cùng của một recipe **đã Published** → bất biến "phải có ≥ 1 bước" bị phá ngay sau khi publish.
- **Phụ trách:** TV2 (TV3 nếu bắt buộc có ảnh, TV4 cho SEO).

### B-03 · Cao · Vòng đời trạng thái công thức không đầy đủ
- **Vị trí:** FR-RCP-005, FR-RCP-006 (tr. 31–32); 7.2 `PublishedAt` (tr. 56); 8.3 (tr. 63–64).
- **Nội dung:** tiêu đề FR-RCP-006 là "Archive / **Unarchive**" nhưng chỉ có luồng và endpoint archive, không có unarchive. Không định nghĩa chuyển trạng thái từ Archived (có publish/unpublish được không?), cũng không nói Draft có archive thẳng được không. 7.2 nói `PublishedAt` "Set khi Status chuyển sang Published" nhưng luồng FR-RCP-005 (bước 6) chỉ set `Status` và `UpdatedAt`; khi unpublish thì `PublishedAt` xử lý thế nào cũng không có.
- **Phụ trách:** TV2.

### B-04 · Cao · Quy tắc hiển thị Draft/Archived khác nhau giữa các mục
- **Vị trí:** FR-RCP-001 (tr. 27–28); FR-CAT-002 (tr. 24–25); 8.3 (tr. 63); Phụ lục C "Draft" (tr. 70); 5.1 `/dashboard/recipes` (tr. 46).
- **Nội dung:**
  - FR-RCP-001 phần mô tả: Author thấy thêm "**Draft/Archived** của chính mình"; bước 4: "Published OR (**Draft** AND A[…] == userId)" — không có Archived.
  - 8.3: `GET /recipes` chỉ "(Published, paginated)".
  - FR-CAT-002: Author thấy Draft của mình (không nhắc Archived, không nhắc Admin) ↔ FR-RCP-001: Admin thấy tất cả.
  - Phụ lục C, mục Draft: "Chỉ Author/Admin thấy" (tức mọi Author, không chỉ chủ sở hữu).
  - Trộn "danh sách công khai" với "bài của tôi" trong cùng endpoint, và không có endpoint riêng cho trang `/dashboard/recipes` (xem G-03).
- **Phụ trách:** TV2, TV3, TV4.

### B-05 · Cao · Xử lý trùng slug và quy tắc đổi slug mâu thuẫn
- **Vị trí:** FR-CAT-003 (tr. 25); FR-CAT-004 (tr. 26); FR-RCP-003 A3 (tr. 30); FR-RCP-004 status (tr. 31); 7.2 Title/Slug (tr. 54–55); NFR-SEO-004 (tr. 45); Phụ lục A dòng 409, Phụ lục B `RECIPE_SLUG_EXISTS` (tr. 67–68).
- **Nội dung:**
  - Category: trùng slug thì **tự thêm hậu tố** "-2", "-3".
  - Recipe (FR-RCP-003 A3): trùng slug thì **409 Conflict**, trong khi 7.2 viết "Unique không bắt buộc (có thể trùng title khác nhau slug)".
  - Phụ lục B tự mâu thuẫn: `RECIPE_SLUG_EXISTS` **409** "Slug đã tồn tại — **tự động thêm suffix** (slug-1, slug-2…)"; định dạng hậu tố cũng khác FR-CAT-003 ("-2" so với "slug-1").
  - Phụ lục A: 409 khi "category slug đã tồn tại" — trái với cơ chế tự thêm hậu tố.
  - Đổi slug: FR-CAT-004 "Slug KHÔNG thay đổi khi đổi tên"; NFR-SEO-004 "không thay đổi sau khi publish" và "Nếu slug thay đổi (draft) → 301 redirect từ slug cũ sang slug mới"; FR-RCP-004 trả "409 – … **slug trùng**" (ngầm hiểu PUT có đổi slug). Không có bảng lưu slug cũ để làm 301, và unpublish → sửa title → publish lại thì slug có đổi không cũng không rõ.
- **Phụ trách:** TV2, TV3, TV4.

### B-06 · Cao · Danh mục: tính duy nhất, độ dài, thứ tự và các trường "mồ côi"
- **Vị trí:** FR-CAT-001/003/004 (tr. 23–26); 7.6 (tr. 58); 8.2 (tr. 62–63).
- **Nội dung:** FR-CAT-003 có điều kiện "Name chưa tồn tại" và mã 409, nhưng luồng chính **không có bước kiểm tra Name** (chỉ kiểm tra slug). FR-CAT-004 không kiểm tra trùng tên, không có 409. Độ dài tên: FR "2–50 ký tự" ↔ DB `varchar(100)`. FR-CAT-001 sắp xếp theo **Name**, trong khi 7.6 có `OrderIndex` là "Thứ tự hiển thị trên navigation". Các trường `ImageUrl`, `OrderIndex` có trong 7.6/8.2 nhưng không có trong body của FR-CAT-003/004 lẫn `CategoryDto`, và không có cách upload ảnh danh mục.
- **Phụ trách:** TV3.

### B-07 · Trung bình · Quy tắc xóa danh mục tự mâu thuẫn và thiếu công cụ
- **Vị trí:** FR-CAT-005 (tr. 26–27).
- **Nội dung:** quy tắc "KHÔNG được xóa danh mục còn chứa công thức" là ràng buộc cứng (chặn thao tác, trả 409), nhưng lại được gọi là "**soft constraint**". Chỉ tính recipe "Published hay Draft", bỏ quên Archived. Bắt Admin "chuyển tất cả công thức sang danh mục khác" nhưng không có chức năng chuyển hàng loạt — phải sửa từng recipe.
- **Phụ trách:** TV3.

### B-08 · Cao · Quy tắc validation mâu thuẫn với ràng buộc CSDL

| Trường | Chương 3 (FR) | Chương 7 (DB) / Chương 8 (API) |
|---|---|---|
| `CookTime` | FR-RCP-003: "prepTime/cookTime/servings **> 0**" | 7.2: `CHECK >= 0`, "0 cho 'No cook' recipes" |
| `Difficulty` | FR-SRCH-002: `Easy\|Medium\|Hard` | 7.2: 1=Easy, 2=Medium, 3=Hard, **4=Expert** |
| Ingredient `Quantity`, `Unit` | FR-RCP-009: "Quantity **> 0**, Unit **không rỗng**" | 7.4: Quantity `NULL` ("nguyên liệu vừa đủ"), Unit `NULL`; 8.6: `quantity?`, `unit?` |
| Ingredient `Name` | FR-RCP-009: 1–**100** ký tự | 7.4: `varchar(200)` |
| Step `Title` | FR-RCP-010: body **không có** title | 7.3: `Title varchar(200) NOT NULL`; 8.5: bắt buộc `title` |
| Recipe `Instructions` | FR-RCP-003: `instructions?` (không bắt buộc) | 7.2: `text NOT NULL`, "legacy field" (trường "legacy" trong một hệ thống v1.0 mới xây) |
| Recipe `Description` | FR-RCP-003: không có quy tắc validation | 7.2: `NOT NULL`, "≤ 2000 ký tự" |
| Nutrition | FR-RCP-003: `SetNutrition(calories, protein, carbs, fat)` — 4 trường | 7.2.1: 6 trường (thêm `Fiber`, `Sodium`); tên `Carbohydrates` ≠ `carbs` |
| FullName / DisplayName | FR-AUTH-001: "không rỗng"; FR-AUTH-007: "2–100 ký tự" | 7.7: `DisplayName varchar(100)` |
| Password | FR-AUTH-001: ≥ 8, 1 hoa, 1 số, 1 ký tự đặc biệt | NFR-SEC-001: như trên **+ 1 chữ thường** |

- **Hệ quả:** gửi dữ liệu hợp lệ theo FR vẫn có thể bị DB từ chối (lỗi 500), hoặc DB cho phép dữ liệu mà API không cho nhập.
- **Phụ trách:** TV1, TV2, TV4.

### B-09 · Cao · Tên trường, tên bảng, tên interface không nhất quán

| Khái niệm | Các biến thể trong SRS |
|---|---|
| Tên người dùng | `fullName` (FR-AUTH-001/006/007) · `DisplayName` (7.7) · `displayName` (8.1) |
| Username | bắt buộc khi đăng ký (FR-AUTH-001) · không có trong 8.1 |
| Thời gian | `prepTimeMinutes`/`cookTimeMinutes` (FR-RCP-003) · `prepTime`/`cookTime` (FR-RCP-004, 7.2) |
| Thời gian của bước | `DurationMinutes` (FR-RCP-010) · `TimerMinutes` (7.3, 8.5) |
| Thứ tự | `SortOrder` (FR-RCP-002, FR-RCP-009) · `OrderIndex` (7.4–7.6, 8.4, 8.6) |
| Ảnh | `url`, `{imgId}` (FR-RCP-008) · `originalUrl`, `{imageId}` (8.4, 7.5) |
| Bảng refresh token | `refresh_tokens` (FR-AUTH-001) · `"RefreshTokens"` (6.4, 7.8) |
| Thu hồi token | `IsRevoked`, `ReplacedByToken` (FR-AUTH-004/005) · `RevokedAt`, `ReplacedByTokenHash` (7.8) |
| Dịch vụ email | `IEmailSender` (5.3) · `IEmailService` (6.2) |
| Policy phân quyền | `RequireAuthorization("Admin")` (FR-CAT-003), `"VerifiedAuthor"` (2.3) · `"AuthorPolicy"`, `"AdminPolicy"` — "không hardcode role string" (NFR-SEC-006) |
| SDK MinIO | "MinIO .NET SDK" (2.1.2), `RemoveObjectAsync()` (FR-FILE-002, tên hàm của MinIO SDK) · `AWSSDK.S3` (5.3, 6.1, 6.2) |

- **Phụ trách:** Chung (quy ước đặt tên), từng module.

### B-10 · Trung bình · Dữ liệu và màn hình không có chức năng tương ứng
- **Vị trí:** 5.1 (tr. 46); 7.3, 7.6, 7.7 (tr. 57–59); Phụ lục B (tr. 68); 1.2.2 (tr. 6).
- **Nội dung:**
  - Trang chủ hiển thị "recipe **nổi bật**" nhưng không có trường `IsFeatured`/`ViewCount` hay quy tắc thế nào là nổi bật.
  - `ApplicationUser.Bio` "hiển thị trên author profile" — không có route, endpoint hay FR cho trang tác giả.
  - `IsActive` "Admin có thể deactivate user (ban)" và mã lỗi `AUTH_ACCOUNT_DISABLED` — không có FR/endpoint để ban; luồng login (FR-AUTH-002) không kiểm tra `IsActive`.
  - `Category.ImageUrl`, `RecipeStep.ImageUrl` — không có cơ chế upload; ảnh của bước không bị xóa khi xóa recipe.
  - 1.2.2 nói "nhiều nền ẩm thực khác nhau" nhưng không có trường hay bộ lọc "ẩm thực".
- **Phụ trách:** TV1, TV2, TV3, TV4.

### B-11 · Trung bình · Phân quyền: vai trò và policy không khớp nhau
- **Vị trí:** 2.3 (tr. 12–13); FR-JOB-001 (tr. 38); Chương 8 "Convention" (tr. 61).
- **Nội dung:** mọi người đăng ký đều tự động là Author, và 2.3 định nghĩa Policy "VerifiedAuthor" "yêu cầu email đã xác nhận" — nhưng không có FR xác nhận email (FR-JOB-001 chỉ ghi "link kích hoạt email (**nếu cần**)"), cũng không nói policy này áp dụng cho endpoint nào → hoặc vô dụng, hoặc chặn toàn bộ Author. Quyền "Xem structured logs" của Admin không có FR. "Guest" được xếp như một vai trò dù thực chất là chưa đăng nhập. Chương 8 ghi "Author⊂Admin", nhưng role của ASP.NET Identity không có kế thừa: Admin được seed phải có thêm role Author, hoặc mọi policy phải chấp nhận cả hai role.
- **Phụ trách:** TV1.

---

## C. Mâu thuẫn hợp đồng API (Chương 3 ↔ 5 ↔ 8 ↔ Phụ lục)

### C-01 · Nghiêm trọng · Hợp đồng đăng ký tài khoản khác nhau hoàn toàn
- **Vị trí:** FR-AUTH-001 (tr. 17–18) ↔ 8.1 (tr. 61).

| | FR-AUTH-001 | 8.1 |
|---|---|---|
| Request | `{ fullName, email, userName, password }` | `{ email, password, displayName }` |
| 201 | `AuthResponseDto { accessToken, refreshToken, expiresAt, user{…} }` — **tự đăng nhập** | `{ userId, email, displayName }` — **không có token** |
| Lỗi validation | 422 | 400 |

- **Hệ quả:** FE và BE của cùng một module sẽ không nói chuyện được với nhau; luồng "auto-login sau đăng ký" có hay không cũng chưa rõ.
- **Phụ trách:** TV1.

### C-02 · Cao · Response của login / me / cập nhật hồ sơ khác nhau
- **Vị trí:** FR-AUTH-002, 006, 007 (tr. 18–23) ↔ 8.1 (tr. 61–62).
- **Nội dung:**
  - Login: FR trả `expiresAt` + `user{…}` ↔ 8.1 trả `expiresIn`, không có `user`.
  - `GET /auth/me`: FR `{ id, fullName, email, userName, avatarUrl, roles, emailConfirmed, createdAt }` ↔ 8.1 `{ id, email, displayName, avatarUrl, bio, roles }`.
  - `PATCH /auth/me`: FR `{ fullName, avatarUrl }` ↔ 8.1 `{ displayName?, avatarUrl?, bio? }`; lỗi 422 ↔ 400.
  - 8.1 thiếu 423 (khóa tài khoản) và 422; FR thiếu 429.
- **Phụ trách:** TV1.

### C-03 · Nghiêm trọng · Đăng nhập Google được mô tả bằng 3 luồng khác nhau
- **Vị trí:** FR-AUTH-003 (tr. 19–20); 5.3 (tr. 47); 6.2 Presentation (tr. 51); 8.1 (tr. 61); Phụ lục B (tr. 68).
- **Nội dung:**
  1. **FR-AUTH-003:** Auth.js v5 xử lý callback **ở Next.js**, sau đó frontend `POST /auth/google` "với Google **ExternalLoginInfo**".
  2. **5.3:** "Redirect URI: **/api/v1/auth/google/callback**", tức callback **ở backend**.
  3. **8.1:** body `{ idToken }` "ID Token từ **Google Sign-In JS SDK**" (thư viện đã bị Google khai tử, xem D-05).
  4. **6.2:** "Auth.js v5 (frontend) **hoặc** ASP.NET Google provider" — chưa chốt.
  - Mã lỗi token không hợp lệ: FR 401 ↔ 8.1 và Phụ lục B (`AUTH_GOOGLE_TOKEN_INVALID`) 400. Điều kiện tiên quyết đặt `ClientSecret` trong appsettings **của backend**, nhưng nếu Auth.js thực hiện đổi code thì secret phải nằm ở **frontend**.
- **Phụ trách:** TV1.

### C-04 · Cao · Phân trang, sắp xếp và "vỏ bọc" response không thống nhất
- **Vị trí:** FR-RCP-001 (tr. 27–28); FR-CAT-002 (tr. 24–25); FR-SRCH-001/003/004 (tr. 36–37); 5.2 "Response Format" (tr. 47); Chương 8 "Pagination" (tr. 61); 8.2, 8.3 (tr. 62–63).

| Khía cạnh | Chương 3 | 5.2 | Chương 8 |
|---|---|---|---|
| Tham số sắp xếp | `sort=-createdAt` | — | `sortBy=createdAt&sortOrder=desc` |
| `pageSize` mặc định | 12 (SRCH-004, CAT-002), ví dụ 10 ở SRCH-001 | 10 | 10 |
| Response | `PagedResult { items, totalCount, page, pageSize, totalPages, hasNextPage, hasPreviousPage }`, không có vỏ bọc | `{ data, meta{ page, pageSize, total } }` | `{ data:[], meta{ page, pageSize, total, totalPages } }` |

- **Thêm:** FR-SRCH-002 nói bộ lọc "tích hợp sẵn vào … FR-SRCH-001", nhưng endpoint của FR-SRCH-001 không có tham số lọc (8.3 lại có `&cate…` bị cắt chữ); `minServings` chỉ xuất hiện ở FR-SRCH-002.
- **Phụ trách:** Chung, TV4.

### C-05 · Cao · Mã HTTP status mâu thuẫn và danh sách chưa đầy đủ
- **Vị trí:** Chương 3 (nhiều FR); 8.1–8.6; Phụ lục A, B (tr. 67–69).
- **Nội dung:**
  - **Lỗi validation:** 422 ở khoảng 15 chỗ trong Chương 3 ↔ **400** ở 8.1, 8.2, 8.4–8.6, Phụ lục A và `VALIDATION_ERROR` (Phụ lục B).
  - **Xung đột đồng thời (concurrency):** 409 (FR-RCP-004) ↔ **422** (Phụ lục A, `RECIPE_CONCURRENCY_CONFLICT`).
  - **Publish thiếu điều kiện:** 422 (FR-RCP-005) ↔ 400 (Phụ lục A, B).
  - Phụ lục A tự nhận "liệt kê **tất cả** HTTP Status Codes được sử dụng" nhưng thiếu **423** (FR-AUTH-002) và **502** (FR-AUTH-003).
  - Các FR yêu cầu đăng nhập (FR-CAT-003/004/005, FR-RCP-004 → 010) không liệt kê 401. FR-RCP-003 A1 "Không có quyền Author/Admin: HTTP 401/403", trong khi mọi user đã đăng ký đều là Author.
- **Phụ trách:** Chung.

### C-06 · Cao · Định dạng lỗi và mã lỗi ứng dụng mâu thuẫn
- **Vị trí:** Chương 8 "Error Format" (tr. 61); Phụ lục B (tr. 67–69); NFR-USE-003 (tr. 42); các FR.
- **Nội dung:**
  - Chương 8: `"type": "about:blank"` ↔ Phụ lục B: mã lỗi `SCREAMING_SNAKE_CASE` đặt **trong trường `type`** (RFC 7807 quy định `type` là một URI tham chiếu).
  - NFR-USE-003 yêu cầu "error messages sử dụng error code (**không hardcode** tiếng Việt/Anh)", nhưng các FR hardcode chuỗi tiếng Việt: `ConflictException("Danh mục còn chứa {count} công thức.")`, `DomainException("Recipe phải có ít nhất 1 bước thực hiện.")`, "Email hoặc mật khẩu không đúng", "Kích thước file vượt quá giới hạn 5MB.".
  - Phụ lục B thiếu nhiều mã mà các FR cần: tài khoản bị khóa (423), trùng username, category không hợp lệ khi tạo recipe, magic bytes không khớp, storage không khả dụng (503), Google không khả dụng (502), không tìm thấy image/step/ingredient.
- **Phụ trách:** Chung.

### C-07 · Cao · Hợp đồng cập nhật công thức và vấn đề CORS
- **Vị trí:** FR-RCP-004 (tr. 30–31); FR-RCP-009 (tr. 35); 8.3 (tr. 63); 5.2 "CORS Headers" (tr. 47).
- **Nội dung:** FR-RCP-004 dùng PUT với **body đầy đủ** + RowVersion trong header `If-Match` "(hoặc trong request body)" ↔ 8.3: PUT với **mọi trường tùy chọn** (thực chất là ngữ nghĩa PATCH) và không nhắc đến concurrency token. FR-RCP-009 cũng PUT "với body fields cần cập nhật". 5.2 khai báo `Access-Control-Allow-Headers: Content-Type, Authorization, X-Correlation-ID` — **không có `If-Match`** → trình duyệt sẽ chặn request ở bước preflight nếu frontend gửi header này; cũng không có `Access-Control-Expose-Headers` → JS không đọc được `ETag`, `Location`, `X-Correlation-ID`, `Retry-After`, `X-RateLimit-*`.
- **Phụ trách:** TV2, Chung.

### C-08 · Cao · API ảnh công thức khác nhau giữa FR và Chương 8
- **Vị trí:** FR-RCP-008 (tr. 33–34) ↔ 8.4 (tr. 64).
- **Nội dung:**
  - Upload trả `{ url, isPrimary }` — **không có id ảnh**, nên client không gọi được set-primary/delete ↔ 8.4 trả `{ imageId, originalUrl, altText, isPrimary }`.
  - Đặt ảnh chính: `PATCH /images/{imageId}/primary` ↔ 8.4: `PATCH /images/{imageId}` cập nhật metadata (`altText`, `isPrimary`, `orderIndex`).
  - Bước 6 của FR dùng `altText` nhưng request chỉ có field `file`; 8.4 có `altText?`, `isPrimary?`. A4 (503) không có trong 8.4.
- **Phụ trách:** TV3.

### C-09 · Cao · API bước thực hiện: ai quyết định StepNumber?
- **Vị trí:** FR-RCP-010 (tr. 35–36) ↔ 8.5 (tr. 64–65).
- **Nội dung:** FR: `StepNumber = Max + 1` do server **tự sinh**, body `{ description, durationMinutes?, imageUrl? }`, và không mô tả luồng PUT (dù endpoint có PUT) ↔ 8.5: client **gửi** `stepNumber` và `title`; PUT có `stepNumber?` (đổi thứ tự?) nhưng không định nghĩa quan hệ với cơ chế tự đánh số lại. `RecipeStep.Create(...)` ở bước 3 bỏ sót `imageUrl`.
- **Phụ trách:** TV2.

### C-10 · Trung bình · Chương 8 thiếu cột và thiếu endpoint
- **Vị trí:** 8.3 (tr. 63–64) và toàn Chương 8.
- **Nội dung:** bảng 8.3 **không có cột Response** cho 9 endpoint recipe. Thiếu các endpoint được nhắc ở nơi khác: unarchive (FR-RCP-006), lấy recipe theo **id** cho trang `/dashboard/recipes/[id]/edit` (API chỉ có GET theo slug), danh sách "recipe của tôi", đổi email/username có OTP (FR-AUTH-007), khóa tài khoản (7.7), upload avatar/ảnh danh mục/ảnh bước.
- **Phụ trách:** Chung.

### C-11 · Thấp · Tiền tố `/api/v1` và xung đột route
- **Vị trí:** 5.2 (tr. 47); 8.7 (tr. 65–66); FR-SRCH-001, FR-RCP-002.
- **Nội dung:** 5.2 viết "Toàn bộ endpoints được tiền tố /api/v1", nhưng `/health*`, `/hangfire`, `/scalar` nằm ngoài tiền tố này. `GET /recipes/search` và `GET /recipes/{slug}` cùng cấp route: ASP.NET Core ưu tiên segment cố định nên chạy được, nhưng recipe nào có slug đúng bằng `search` sẽ không bao giờ truy cập được.
- **Phụ trách:** TV2, TV4.

---

## D. Sai hoặc không khả thi về kỹ thuật

### D-01 · Nghiêm trọng · `RowVersion bytea [Timestamp]` không hoạt động trên PostgreSQL
- **Vị trí:** 7.1, 7.2 (tr. 54–56); 3.3 (tr. 27); FR-RCP-004 bước 6 (tr. 30).
- **Nội dung:** `[Timestamp] byte[]` là cơ chế `rowversion` **của SQL Server**. PostgreSQL không tự sinh giá trị cho cột `bytea`, nên cột `NOT NULL` này hoặc làm INSERT lỗi, hoặc không bao giờ thay đổi (concurrency check vô tác dụng). Npgsql khuyến nghị dùng cột hệ thống `xmin` ánh xạ sang thuộc tính `uint` có `[Timestamp]`. Ngoài ra FR-RCP-004 bước 6 "Kiểm tra RowVersion: DbContext sẽ ném DbUpdateConcurrencyException" đặt **trước** bước cập nhật, trong khi exception chỉ xảy ra lúc `SaveChanges`, và chỉ khi original value được gán bằng giá trị client gửi lên.
- **Phụ trách:** Chung (BaseEntity), TV2.

### D-02 · Nghiêm trọng · Full-text search tiếng Việt viết sai về PostgreSQL
- **Vị trí:** 1.2.2 (tr. 7); FR-SRCH-001 (tr. 36–37); 7.2 `SearchVector` (tr. 56); CONS-006 (tr. 15).
- **Nội dung:**
  1. `EF.Functions.ToTsQuery("vietnamese", …)`: PostgreSQL **không có sẵn** cấu hình text search `vietnamese` → lỗi runtime nếu không tự tạo cấu hình riêng.
  2. "SearchVector (**computed column**) được tự động cập nhật bởi PostgreSQL **trigger**" — generated column và trigger là hai cơ chế khác nhau; `unaccent()` không phải hàm IMMUTABLE nên không dùng trực tiếp trong generated column được.
  3. "Hỗ trợ tìm kiếm **gần đúng** với unaccent" — `unaccent` chỉ bỏ dấu; tìm gần đúng (sai chính tả) cần `pg_trgm`. `pg_trgm` được liệt kê là bắt buộc nhưng không được dùng ở luồng nào.
  4. Tự ghép `"pho:* & bo:*"` rồi đưa vào `to_tsquery` sẽ gây lỗi cú pháp khi người dùng nhập ký tự đặc biệt; từ khóa tìm kiếm cũng phải được `unaccent` giống như dữ liệu.
  5. Chỉ index Title + Description — tìm theo nguyên liệu ("thịt bò") sẽ không ra kết quả.
  6. CONS-006 cấm raw SQL, trong khi extension, text search configuration, trigger, GIN index đều bắt buộc phải viết SQL trong migration (xem D-23).
- **Phụ trách:** TV4.

### D-03 · Nghiêm trọng · Cache response nhưng nội dung lại phụ thuộc người xem
- **Vị trí:** FR-RCP-001, FR-RCP-002 (tr. 27–29); FR-CAT-002 (tr. 24); 5.1 (tr. 46).
- **Nội dung:** Output Cache "vary by query string", key = `{path}?{queryString}`, trong khi kết quả khác nhau theo người xem (Author thấy Draft của mình, Admin thấy tất cả, Guest bị 403 với Draft). Policy mặc định của ASP.NET Core Output Caching **không cache request có xác thực**; nếu nhóm tự viết policy để cache cả những request đó thì Draft sẽ bị lộ cho Guest, hoặc response 403 bị cache cho chính chủ bài. SRS không nói rõ điều này. Tương tự, `/categories/[slug]` được render bằng ISR (5.1) — trang tĩnh dùng chung cho mọi người — nên không thể hiển thị "Draft của chính mình" như FR-CAT-002 yêu cầu.
- **Phụ trách:** TV2, TV3, TV4.

### D-04 · Nghiêm trọng · Bốn cơ chế cache với TTL mâu thuẫn nhau

| Dữ liệu | FR (Chương 3) | NFR-PERF-003 | Khác |
|---|---|---|---|
| Danh sách danh mục | `IMemoryCache`, TTL 60 phút, **sliding** (FR-CAT-001; 3.2 ghi 1 giờ) | Redis, TTL **30 phút** | NFR-SCALE-001: "**không** in-memory IMemoryCache"; 6.3: `CachingBehavior` kiểm tra **Redis** |
| Chi tiết recipe | Output Cache, **60 phút** (FR-RCP-002) | Redis cache-aside, **5 phút** | ISR `revalidate=300` (5.1) |
| Danh sách recipe | Output Cache, **15 phút** (FR-RCP-001) | — | ISR / SSR (5.1) |
| Kết quả tìm kiếm | "KHÔNG cache (…) **hoặc** cache ngắn (5 phút)" (FR-SRCH-001) | Redis, **1 phút** | — |
| Bộ đếm rate limit | ASP.NET Core Rate Limiting (NFR-SEC-003, lưu trong bộ nhớ) | — | 6.1: "Rate limiting counters" trong **Redis** |

- **Hệ quả thêm:** (1) Sliding expiration trên key `categories:all` được đọc liên tục → **không bao giờ hết hạn**, và cache chỉ bị xóa khi CRUD danh mục, không bị xóa khi publish/unpublish/xóa recipe → `recipeCount` sai vô thời hạn. (2) Output Cache 60 phút chồng lên ISR 300 giây mà không có cơ chế revalidate chủ động → sửa bài xong có thể mất tới ~65 phút mới thấy. (3) Ngưỡng hiệu năng NFR-PERF-001 được đặt với điều kiện "Redis hit rate ≥ 80%", trong khi phần lớn cache trong FR không dùng Redis.
- **Phụ trách:** Chung, TV2, TV3, TV4.

### D-05 · Nghiêm trọng · Lỗ hổng trong luồng đăng nhập Google
- **Vị trí:** FR-AUTH-003 (tr. 19–20); 8.1 (tr. 61).
- **Nội dung:**
  1. Frontend gửi "Google **ExternalLoginInfo**" (gồm providerKey, email) và backend tin luôn → bất kỳ ai cũng có thể tự `POST` một email/providerKey giả để **đăng nhập thành người khác**. Backend phải tự xác minh ID token (chữ ký, `aud`, `iss`, `exp`).
  2. Tự động liên kết Google với tài khoản local "nếu email đã tồn tại" mà không kiểm tra `email_verified` → nguy cơ chiếm tài khoản.
  3. 8.1 dựa vào "Google Sign-In JS SDK" — thư viện này đã bị Google khai tử, thay bằng Google Identity Services.
  4. User tạo qua Google không được gán `userName` hợp lệ (Identity yêu cầu username duy nhất) và không nhận email chào mừng (FR-JOB-001 chỉ kích hoạt sau FR-AUTH-001).
- **Phụ trách:** TV1.

### D-06 · Cao · Luồng đăng nhập local sai logic lockout và chống dò tài khoản
- **Vị trí:** FR-AUTH-001 A1 (tr. 18); FR-AUTH-002 (tr. 18–19).
- **Nội dung:**
  1. Điều kiện tiên quyết "LockoutEnabled = **false** hoặc chưa đến lockout deadline": `LockoutEnabled = false` nghĩa là **tắt** tính năng khóa cho user đó — trái với A3 (khóa 15 phút sau 5 lần sai).
  2. Kiểm tra lockout (bước 6) nằm **sau** kiểm tra mật khẩu (bước 5).
  3. `UserManager.CheckPasswordAsync` **không tăng** `AccessFailedCount`; muốn A3 hoạt động phải gọi `AccessFailedAsync` hoặc dùng `SignInManager.CheckPasswordSignInAsync(..., lockoutOnFailure: true)`.
  4. A1 cố tình trả thông báo chung để chống dò tài khoản, nhưng A2 trả **423** kèm thời gian mở khóa (lộ rằng tài khoản tồn tại), và đăng ký trả 409 "Email đã tồn tại" — lập trường chống dò tài khoản không nhất quán.
  5. Không kiểm tra `IsActive` (tài khoản bị ban vẫn đăng nhập được).
  6. Phần mô tả nói "refresh token cũ … đánh dấu đã sử dụng" — việc này thuộc về luồng refresh (FR-AUTH-004), không phải login.
- **Phụ trách:** TV1.

### D-07 · Cao · Đặc tả refresh token mâu thuẫn và dễ tự đăng xuất người dùng
- **Vị trí:** FR-AUTH-001 (tr. 18); FR-AUTH-004, FR-AUTH-005 (tr. 20–22); NFR-SEC-002 (tr. 41); 7.8 (tr. 59–60); Phụ lục C (tr. 71).
- **Nội dung:**
  - Độ dài token: **512-bit** (FR-AUTH-001) ↔ **128-bit** (NFR-SEC-002).
  - Lưu trữ: FR-AUTH-004 tìm token "trong database" theo giá trị gốc, set `IsRevoked`, `ReplacedByToken = newToken` ↔ 7.8/NFR-SEC-002: chỉ lưu `TokenHash` (SHA-256), có `RevokedAt`, `ReplacedByTokenHash`, **không có** `IsRevoked`.
  - Phát hiện dùng lại token: FR-AUTH-004 "**có thể** … (paranoid mode)" ↔ NFR-SEC-002 và Phụ lục C: **bắt buộc** "revoke toàn bộ family" — nhưng 7.8 không có `FamilyId`.
  - FR-AUTH-004 A3 coi mọi token đã revoke là "Reuse Attack", trong khi logout (FR-AUTH-005) cũng revoke → không phân biệt được token đã đăng xuất với token bị đánh cắp, trừ khi kiểm tra thêm `ReplacedByTokenHash`.
  - Bước 5 gán `ReplacedByToken = newToken` trước khi bước 7 tạo token mới.
  - **Không xử lý refresh đồng thời:** hai tab (hoặc callback `jwt` của Auth.js chạy song song) cùng dùng một refresh token → request thứ hai bị coi là tấn công → toàn bộ phiên bị thu hồi, người dùng bị đăng xuất ngẫu nhiên.
  - Không có job dọn token hết hạn.
- **Phụ trách:** TV1.

### D-08 · Cao · Chưa chốt nơi lưu token; các lựa chọn đang mâu thuẫn nhau
- **Vị trí:** FR-AUTH-005 (tr. 21); 5.2 "Authentication" (tr. 47); NFR-SEC-005 (tr. 41); FR-AUTH-003, 6.1 (Auth.js v5).
- **Nội dung:** FR-AUTH-005 "localStorage/cookie"; 5.2 "Refresh token: trong request body (**không dùng cookie** để tránh CSRF)"; NFR-SEC-005 "Cookie: SameSite=Strict, Secure=true (**nếu** dùng cookie cho refresh token)"; FR-AUTH-003/6.1 dùng Auth.js v5 (Auth.js quản lý phiên bằng cookie). Để refresh token trong localStorage thì chỉ cần một lỗi XSS là bị đánh cắp; cookie `SameSite=Strict` thì không được gửi khi người dùng bấm link từ trang khác (ví dụ từ Google) sang → lần tải trang đầu tiên hiển thị như chưa đăng nhập.
- **Phụ trách:** TV1.

### D-09 · Cao · Logout tự mâu thuẫn về access token hết hạn
- **Vị trí:** FR-AUTH-005 (tr. 21–22); 8.1 (tr. 62).
- **Nội dung:** điều kiện tiên quyết yêu cầu "access token **hợp lệ**" và luồng chính dùng middleware JWT; A2 lại "Access token đã hết hạn: **vẫn cho phép logout** nếu refresh token hợp lệ". Endpoint có `RequireAuthorization` sẽ trả 401 trước khi handler kịp chạy, nên A2 không thể xảy ra như mô tả.
- **Phụ trách:** TV1.

### D-10 · Cao · SRS tự vi phạm Clean Architecture mà chính nó quy định
- **Vị trí:** CONS-001 (tr. 14); NFR-MAINT-004 (tr. 43–44); 6.2 (tr. 50–51); 6.3 (tr. 52); 6.4 (tr. 52); 7.7 (tr. 58); FR-AUTH-001/002, FR-RCP-004, FR-RCP-007, FR-FILE-001.
- **Nội dung:**
  - Domain có 3 quy định khác nhau: "Tầng Domain không được phụ thuộc **bất kỳ thư viện ngoài nào**" (CONS-001) / "Không có NuGet dependencies (chỉ .NET BCL)" (6.2) / "Không có nuget packages **ngoài FluentValidation**" (NFR-MAINT-004 — trái với CONS-008 đặt validation ở tầng Application).
  - Domain chứa `ApplicationUser : IdentityUser` (6.2, 7.7) → Domain phụ thuộc gói ASP.NET Core Identity.
  - Handler ở tầng Application gọi trực tiếp `UserManager` (Identity), `BackgroundJob.Enqueue` (Hangfire, lớp static), `IAuthorizationService`; interface `IFileStorageService.UploadAsync(IFormFile…)` dùng kiểu của ASP.NET Core → vi phạm "Application layer: chỉ depend vào Domain" (NFR-MAINT-004).
  - Các luồng FR dùng `IUnitOfWork`, nhưng 6.2 chỉ liệt kê `IRepository<T>` trong Domain, không có `IUnitOfWork`.
  - Pipeline behaviors: 6.2 gồm Validation, Logging, Caching, **Performance** ↔ 6.3 gồm Logging, Validation, Caching, **CacheInvalidation**.
  - "Tất cả entities kế thừa BaseEntity" (6.4, 7) — nhưng `ApplicationUser` đã kế thừa `IdentityUser` (C# không đa kế thừa, Id là `varchar(450)` chứ không phải `uuid`), còn `RefreshToken` (7.8) không có `UpdatedAt`/`IsDeleted`/`RowVersion`.
  - Bảng 6.4 ghi `"RecipeImages" (owned — cột trong Recipes)` — thực thể owned là `RecipeNutrition`, không phải `RecipeImages`.
- **Phụ trách:** Chung, TV1.

### D-11 · Cao · Readiness probe làm sập hệ thống khi Redis lỗi
- **Vị trí:** FR-OBS-001 (tr. 39); 8.7 (tr. 66); 2.6.2 (tr. 15–16); NFR-REL-002 (tr. 43).
- **Nội dung:** `/health/ready` fail khi **Redis** down "→ Kubernetes/Nginx ngừng route traffic", nhưng 2.6.2 và NFR-REL-002 nói Redis down thì "hệ thống tiếp tục hoạt động … fallback database". Hơn nữa triển khai là Docker Compose chứ không phải Kubernetes, và Nginx bản mã nguồn mở không có active health check (đó là tính năng của NGINX Plus) → câu "Readiness probe sẽ fail, Nginx trả 503" (2.6.2) không tự xảy ra.
- **Phụ trách:** TV4.

### D-12 · Cao · Mục tiêu triển khai không nhất quán
- **Vị trí:** 1.2.1 (tr. 6); 2.6.1 (tr. 15); 6.5 (tr. 52–53) ↔ NFR-REL-001, NFR-SEC-007, NFR-PERF-002, NFR-SCALE-001/003 (tr. 40–44).
- **Nội dung:** phần mô tả nói "Docker Compose + Nginx", "phù hợp với **single-server** deployment". Phần NFR lại đòi "Kubernetes readiness probe", "Kubernetes Secrets", "MinIO: **Distributed Mode (4+ nodes)**", "thêm instance tăng tuyến tính", "Nginx: load balancer upstream pool cho nhiều API instances", "CDN (Cloudflare)" (không có trong danh sách hệ thống ngoài). Danh sách container ở NFR-SCALE-003 còn quên Next.js frontend.
- **Phụ trách:** Chung, TV4.

### D-13 · Cao · Rate limiting theo IP sẽ chặn nhầm cả hệ thống
- **Vị trí:** NFR-SEC-003 (tr. 41); 5.2 "Rate Limit Headers" (tr. 47); 6.1 (tr. 50).
- **Nội dung:**
  1. API đứng sau Nginx: nếu không cấu hình `ForwardedHeaders` thì mọi client có chung IP của container Nginx → toàn hệ thống dùng chung một hạn mức.
  2. Các request SSR/ISR và các lời gọi server-to-server của Auth.js đều đi ra từ **IP của server Next.js** → cả site dùng chung 100 req/phút, và hạn mức auth **10 req/phút** áp dụng chung cho mọi người dùng.
  3. `/auth/*` gồm cả `/auth/me` và `/auth/refresh` (được gọi thường xuyên) → dễ bị 429.
  4. Mạng trường học dùng chung NAT → cả lớp chung một IP khi demo.
  5. Header `X-RateLimit-*` không được middleware có sẵn trả về, phải tự viết.
- **Phụ trách:** Chung, TV1.

### D-14 · Cao · Kiểm tra file upload và lưu trữ ảnh có lỗ hổng
- **Vị trí:** CONS-007 (tr. 15); FR-RCP-008 (tr. 33–34); FR-FILE-001/002 (tr. 37–38); NFR-SEC-004 (tr. 41); 2.4.1 MinIO (tr. 13); 5.3 MinIO (tr. 48).
- **Nội dung:**
  1. Cho phép 4 định dạng nhưng magic bytes chỉ mô tả JPEG và PNG. "Đọc **4 bytes đầu**" không đủ để nhận diện WebP (`RIFF....WEBP`, cần 12 byte) và AVIF (box `ftyp` bắt đầu từ offset 4).
  2. FR-FILE-001 "**Preserve MIME type gốc**" (tin `Content-Type` do client gửi) ↔ NFR-SEC-004 "không tin vào Content-Type header". Phần mở rộng file cũng lấy từ tên file do client đặt.
  3. 5.3 "Presigned URL cho direct browser upload (optional)" → file đi thẳng lên MinIO, **bỏ qua** toàn bộ bước kiểm tra MIME/size/magic bytes ở server.
  4. Bucket `public-read` → ảnh của recipe **Draft** cũng truy cập công khai được nếu biết URL.
  5. `UploadAsync` "trả về URL công khai", trong khi endpoint cấu hình là `minio:9000` (tên host nội bộ của Docker) → trình duyệt không mở được nếu không có cấu hình public endpoint riêng.
  6. Lưu **URL đầy đủ** thay vì object key, và FR-FILE-002 "trích xuất object name từ URL" → đổi domain/CDN phải migrate dữ liệu, cách tách chuỗi dễ vỡ.
- **Phụ trách:** TV3.

### D-15 · Cao · Phiên bản ảnh và việc dọn file bị bỏ sót
- **Vị trí:** FR-JOB-002 (tr. 38); FR-RCP-007, FR-RCP-008 (tr. 32–34); NFR-SEO-002 (tr. 44); NFR-PERF-005 (tr. 41); 7.5 (tr. 58).
- **Nội dung:** FR-JOB-002 tạo `medium` và `thumbnail`, nhưng FR-RCP-007/008 chỉ xóa URL ảnh gốc → hai phiên bản còn lại **thành file mồ côi** trên MinIO; ảnh của bước (`RecipeStep.ImageUrl`) cũng không bị xóa. NFR-SEO-002 cần `og:image` **1200×630** nhưng không job nào tạo kích thước này. Thumbnail 300×300 (1:1) và medium 800×600 (4:3) khác tỉ lệ nhưng không nói crop hay giữ nguyên tỉ lệ. Việc resize trùng với tối ưu ảnh của `next/image` (NFR-PERF-005). Job được enqueue sau khi commit DB, không cùng transaction → nếu tiến trình chết giữa chừng thì còn file rác.
- **Phụ trách:** TV3.

### D-16 · Cao · Hangfire: mô tả sai và thiếu cơ chế xác thực Dashboard
- **Vị trí:** 2.6.2 (tr. 16); 3.6 (tr. 38); FR-JOB-001/003 (tr. 38–39); NFR-REL-002 (tr. 43); NFR-SCALE-001 (tr. 44); 5.3 (tr. 48).
- **Nội dung:**
  1. 2.6.2: khi Hangfire không khả dụng thì "Fire-and-forget jobs **sẽ bị mất**" — sai, vì 3.6/5.3 dùng PostgreSQL làm storage nên job vẫn được lưu và sẽ chạy lại.
  2. Dashboard `/hangfire` "chỉ Admin": trình duyệt mở trang dashboard không tự gửi header `Authorization: Bearer`, nên cần cơ chế xác thực khác (cookie, basic auth…) — SRS không mô tả.
  3. Số lần retry: NFR-REL-002 "tối đa 3" ↔ FR-JOB-003 "2 lần"; chu kỳ "1 phút, 5 phút, 30 phút" phải cấu hình `AutomaticRetry` riêng.
  4. NFR-SCALE-001 dùng RedLock cho job sitemap — thừa, vì Hangfire đã có cơ chế chặn chạy trùng; lại khiến job phụ thuộc Redis.
- **Phụ trách:** TV1, TV3, TV4.

### D-17 · Cao · Sitemap: dùng API đã bị Google khai tử, nơi lưu chưa chốt
- **Vị trí:** FR-JOB-003 (tr. 38–39); NFR-SEO-003 (tr. 44–45); 5.3 "Google Search Console" (tr. 48).
- **Nội dung:** endpoint ping `https://www.google.com/ping?sitemap=` đã bị Google khai tử từ 2023 → gọi vô ích. "Upload sitemap.xml lên MinIO **hoặc** lưu vào wwwroot": wwwroot trong container sẽ mất khi build lại container và không chia sẻ được giữa các instance (trái với định hướng scale ngang); sitemap nằm trên MinIO thì khác origin với site, phải khai báo qua robots.txt. Backend không nắm cấu trúc URL của frontend, trong khi Next.js có sẵn cơ chế sinh sitemap. `<changefreq>`/`<priority>` bị Google bỏ qua.
- **Phụ trách:** TV4.

### D-18 · Cao · Cấu hình Docker Compose có nhiều lỗi sẽ gặp ngay khi chạy
- **Vị trí:** 6.1 (tr. 50); 6.5 (tr. 52–53); 5.2 (tr. 47).
- **Nội dung:**
  1. 6.1: Nginx trỏ tới "Backend API **:5000**", nhưng container `api` lắng nghe cổng **8080** (`5000:8080`) → trong mạng Docker phải gọi `api:8080`.
  2. Mọi cổng hạ tầng đều được publish ra host (5432, 6379, 9000, 9001), kể cả ở production; Redis không có mật khẩu.
  3. `minio/minio:latest`, `datalust/seq:latest` không ghim phiên bản (và image MinIO đã bị gỡ, xem F-01).
  4. Container Seq bắt buộc `ACCEPT_EULA=Y` → thiếu biến này thì không khởi động được.
  5. `mailhog/mailhog`: dự án đã ngừng phát triển (bản cuối năm 2020), gây khó khăn khi chạy trên máy ARM.
  6. Không có service cho OTel Collector, Jaeger/Tempo, Grafana hay Elasticsearch, dù NFR và 5.3 yêu cầu ở production.
  7. Next.js cần **hai** địa chỉ API (`http://api:8080` khi gọi từ server, URL public khi gọi từ trình duyệt) — SRS chỉ định nghĩa một.
  8. Các trang ISR fetch API lúc `next build`, nhưng khi build image Docker thì API chưa chạy → build lỗi, trừ khi cấu hình render lúc runtime.
  9. Service `api` dùng `.env.production` ngay trong bảng của môi trường dev.
- **Phụ trách:** Chung.

### D-19 · Cao · "Node.js chỉ cần lúc build" là sai
- **Vị trí:** 2.4.1 (tr. 13).
- **Nội dung:** "Node.js 20 LTS (build only) … production dùng standalone output". Standalone output vẫn chạy bằng `node server.js`; SSR/ISR (5.1) bắt buộc có Node.js runtime ở production.
- **Phụ trách:** Chung.

### D-20 · Trung bình · EF Core / Npgsql: API không tồn tại và cấu hình connection pool
- **Vị trí:** FR-RCP-002 bước 3 (tr. 29); NFR-PERF-004 (tr. 40–41); NFR-SCALE-002 (tr. 44).
- **Nội dung:** `.IncludeOwned(...)` không tồn tại trong EF Core (owned entity được load tự động). Include nhiều collection cùng lúc gây "cartesian explosion" → nên dùng `AsSplitQuery()` hoặc projection (NFR-PERF-004 viết "bắt buộc dùng .Include()/.ThenInclude() và projection" như thể hai thứ luôn đi cùng nhau). NFR-SCALE-002 viết "Read replica (tùy chọn): EF Core **split queries** + IQueryable routing qua IDbContextFactory" — split query không liên quan gì đến read replica. Pool "max 100 connections/instance" trong khi `max_connections` mặc định của PostgreSQL là 100 → API + Hangfire + nhiều instance sẽ vượt giới hạn.
- **Phụ trách:** Chung, TV2.

### D-21 · Trung bình · Đánh số lại bước thực hiện đụng ràng buộc UNIQUE
- **Vị trí:** FR-RCP-010 (tr. 35–36); 7.3 (tr. 57).
- **Nội dung:** `UNIQUE(RecipeId, StepNumber)` là ràng buộc không trì hoãn (non-deferrable) → cập nhật lại số thứ tự nhiều dòng trong một lần `SaveChanges` có thể vi phạm tùy thứ tự lệnh; đổi chỗ hai bước thì chắc chắn vi phạm. Nếu áp dụng soft delete (B-01), bước đã xóa vẫn giữ số cũ → xung đột chắc chắn xảy ra. Hai request thêm bước cùng lúc tính `Max + 1` giống nhau → trùng số.
- **Phụ trách:** TV2.

### D-22 · Trung bình · "Chỉ 1 ảnh chính" không được đảm bảo ở DB
- **Vị trí:** 7.5 (tr. 58); FR-RCP-008 bước 10, 15 (tr. 34).
- **Nội dung:** không có partial unique index (`WHERE "IsPrimary"`), nên hai request đồng thời có thể tạo ra 2 ảnh chính. "Ảnh đầu tiên còn lại" không nói sắp theo tiêu chí nào (`OrderIndex`? `CreatedAt`?).
- **Phụ trách:** TV3.

### D-23 · Trung bình · CONS-006 cấm raw SQL nhưng hệ thống bắt buộc phải có
- **Vị trí:** CONS-006 (tr. 15); FR-SRCH-001 (tr. 36); 2.4.1 extensions (tr. 13); 7.2 (tr. 54–56).
- **Nội dung:** `CREATE EXTENSION unaccent/pg_trgm`, `CREATE TEXT SEARCH CONFIGURATION`, trigger cập nhật `SearchVector`, GIN index, partial index… đều phải viết bằng `migrationBuilder.Sql(...)`. Cần ghi rõ ngoại lệ "SQL trong migration được phép".
- **Phụ trách:** TV4.

### D-24 · Trung bình · Chiến lược index mâu thuẫn với NFR-PERF-004
- **Vị trí:** NFR-PERF-004 (tr. 40); 7.2 (tr. 54–56).
- **Nội dung:** NFR yêu cầu "mọi WHERE/ORDER BY column đều có B-tree index", nhưng 7.2 không có index cho `CreatedAt` (sắp xếp mặc định), `CookTime` (lọc/sắp xếp), `Servings` (`minServings`), `Title` (sắp xếp). Quy tắc "index mọi cột" cũng không hợp lý; index kết hợp như `(Status, CreatedAt)` mới thực sự hữu ích.
- **Phụ trách:** TV4.

### D-25 · Trung bình · Đăng ký tài khoản không có transaction và bỏ sót kiểm tra
- **Vị trí:** FR-AUTH-001 (tr. 17–18).
- **Nội dung:** `CreateAsync` → `AddToRoleAsync` → lưu refresh token → enqueue job chạy tuần tự, **không có transaction** → lỗi giữa chừng để lại user không có role. A4 "EF Core ném DbUpdateException" khi mất kết nối DB — mất kết nối sẽ ném exception khác, và điều kiện tiên quyết đã giả định DB đang kết nối. Không kiểm tra trùng `userName` → Identity trả `DuplicateUserName` và bị map thành 422 thay vì 409.
- **Phụ trách:** TV1.

### D-26 · Trung bình · Hồ sơ: AvatarUrl tự do và quy trình OTP "ma"
- **Vị trí:** FR-AUTH-007 (tr. 23); 7.7 (tr. 58).
- **Nội dung:** `avatarUrl` là URL bất kỳ, không upload → `next/image` phải cho phép mọi domain (`remotePatterns`), CSP `img-src` phải mở rộng, dễ bị dùng ảnh tracking hoặc nội dung HTTP lẫn trong HTTPS (mixed content). "Email và UserName không thể thay đổi qua endpoint này (đây là quy trình riêng có xác nhận OTP)" — quy trình OTP này không được đặc tả ở đâu cả.
- **Phụ trách:** TV1.

### D-27 · Thấp · Cùng một tình huống nhưng trả mã khác nhau
- **Vị trí:** FR-AUTH-004 A4 (tr. 21); FR-AUTH-006 A1 (tr. 22).
- **Nội dung:** user đã bị xóa sau khi token được cấp: refresh trả **401**, `GET /auth/me` trả **404**.
- **Phụ trách:** TV1.

### D-28 · Trung bình · Kịch bản concurrency mô tả sai; sửa phần con không được phát hiện
- **Vị trí:** 3.3 (tr. 27); FR-RCP-008/009/010.
- **Nội dung:** "hai Author cùng sửa một recipe" không thể xảy ra (chỉ chủ sở hữu được sửa) — tình huống thật là chủ bài với Admin, hoặc một người mở hai tab. Việc sửa steps/ingredients/images không làm thay đổi RowVersion của Recipe → sửa đồng thời các phần con sẽ không bị phát hiện.
- **Phụ trách:** TV2.

### D-29 · Thấp · Phạm vi kiểm tra quyền sở hữu mô tả thiếu
- **Vị trí:** NFR-SEC-006 (tr. 42); FR-RCP-002 A2 (tr. 29).
- **Nội dung:** NFR ghi "Author chỉ **xóa** recipe của mình", trong khi sửa/publish/archive/ảnh/bước/nguyên liệu cũng phải kiểm tra quyền sở hữu; "double-check user ID trước khi commit" quá mơ hồ. Trả 403 cho Draft của người khác làm lộ rằng slug đó tồn tại (có thể cân nhắc trả 404).
- **Phụ trách:** TV2.

### D-30 · Thấp · HS256 kết hợp xoay khóa 90 ngày
- **Vị trí:** NFR-SEC-002, NFR-SEC-007 (tr. 41–42); 6.2 (tr. 51).
- **Nội dung:** xoay khóa HS256 mà không có `kid` và giai đoạn chạy song song khóa cũ/mới → toàn bộ phiên bị vô hiệu khi đổi khóa. Nếu frontend cần tự xác minh JWT thì phải chia sẻ khóa bí mật. `System.IdentityModel.Tokens.Jwt` là thư viện cũ; Microsoft khuyến nghị `Microsoft.IdentityModel.JsonWebTokens`.
- **Phụ trách:** TV1.

### D-31 · Trung bình · Logging: đích production và ngưỡng cảnh báo không thống nhất
- **Vị trí:** FR-OBS-002/003 (tr. 39); 2.1.2 (tr. 11); 5.3 (tr. 48); 6.1, 6.3 (tr. 50–52); NFR-PERF-004 (tr. 40–41); 5.2 (tr. 47).
- **Nội dung:** log ở production được gửi đi đâu có 3 phiên bản: chỉ Console/File (FR-OBS-002, Seq chỉ dùng ở dev) / "Elas… Monitor" (5.3, bị cắt chữ) / Grafana (6.1). File sink "rolling daily" nằm trong container sẽ mất khi container bị tạo lại. Ngưỡng chậm: LoggingBehavior > **500 ms** (6.3, FR-OBS-002) ↔ "cảnh báo khi query > **100ms** (Serilog performance behavior)" (NFR-PERF-004) — một MediatR behavior không đo được thời gian của từng câu SQL (cần EF Core interceptor). "Serilog MDC" — MDC là khái niệm của log4j; Serilog dùng `LogContext`.
- **Phụ trách:** TV4.

### D-32 · Thấp · Mô tả hệ thống ngoài không chính xác
- **Vị trí:** 2.1.1, 2.1.2 (tr. 11–12).
- **Nội dung:** Hangfire (thư viện chạy trong tiến trình) bị xếp vào "Hệ thống Ngoài"; Redis là "Distributed Cache & **Session Store**" dù JWT stateless và hệ thống không có session. Sơ đồ bối cảnh thiếu Nginx, SMTP, Seq/OTel, CDN.
- **Phụ trách:** Chung.

---

## E. NFR sai số liệu / không đo được / không thực tế

### E-01 · Cao · Tính sai thời gian downtime của SLA 99.5%
- **Vị trí:** NFR-REL-001 (tr. 43).
- **Nội dung:** "uptime ≥ 99.5% (≈ 3.65 giờ downtime/năm)". Đúng phải là 0.5% × 8.760 giờ ≈ **43,8 giờ/năm**. 3,65 giờ/năm tương ứng khoảng 99,96%.
- **Phụ trách:** TV4.

### E-02 · Trung bình · Chỉ tiêu hiệu năng tự mâu thuẫn và không kiểm chứng được
- **Vị trí:** NFR-PERF-001/002/003 (tr. 40).
- **Nội dung:** "p99 ≤ 1000ms — không vượt quá 1 giây **trong mọi trường hợp**" — p99 vốn cho phép 1% request vượt ngưỡng; ngưỡng này lại áp dụng cả cho upload ảnh 5 MB. "Đo trong môi trường production với tải thực tế" — đồ án môn học không có production. "Thêm instance tăng tuyến tính" không đo được. Điều kiện "Redis hit rate ≥ 80%" không khớp với các cơ chế cache trong FR (D-04).
- **Phụ trách:** TV4.

### E-03 · Trung bình · Yêu cầu bảo trì quá nặng so với nhóm 4 sinh viên, thiếu CI
- **Vị trí:** NFR-MAINT-001/002/003 (tr. 43).
- **Nội dung:** StyleCop + SonarAnalyzer + "không có compiler warnings" + ≥ 80% coverage + ADR cho **mọi** quyết định. ESLint "Airbnb ruleset" chậm hỗ trợ flat config của ESLint 9, trong khi Next.js dùng `eslint-config-next`. SRS nhắc "build CI", "Lighthouse CI" nhưng không mô tả hệ thống CI nào (2.4.2 không có).
- **Phụ trách:** Chung.

### E-04 · Trung bình · SEO đòi hỏi thứ nằm ngoài phạm vi
- **Vị trí:** NFR-SEO-001/002 (tr. 44); 1.2.3 (tr. 7).
- **Nội dung:** kỳ vọng Rich Snippet có "**star rating**", nhưng hệ thống đánh giá sao đã bị loại khỏi phạm vi (1.2.3) → không có `aggregateRating`. "Pass 100%" cần có ảnh (B-02). `<title>` ≤ 60 ký tự trong khi Title recipe dài tới 200 ký tự, nhưng không có quy tắc cắt ngắn.
- **Phụ trách:** TV4.

### E-05 · Thấp · Khả năng sử dụng: chi tiết lệch với công cụ đã chọn
- **Vị trí:** NFR-USE-001/002/004 (tr. 42).
- **Nội dung:** breakpoint 768/1200 px không trùng với breakpoint mặc định của Tailwind (768/1024/1280) → phải tùy biến cấu hình. WCAG 2.1 (bản hiện hành là 2.2). "Optimistic update" cho **mọi** thao tác ghi xung đột với validation/409 phía server (UI phải rollback liên tục). Thanh tiến trình upload cần XHR/Axios (`fetch` không hỗ trợ theo dõi tiến trình upload).
- **Phụ trách:** Chung (frontend).

### E-06 · Thấp · Yêu cầu phần cứng mâu thuẫn và phi thực tế
- **Vị trí:** 2.4.1 (tr. 13) ↔ 5.4 (tr. 48–49).
- **Nội dung:** ổ cứng production tối thiểu 20 GB (2.4.1) ↔ 50 GB (5.4). "Bandwidth ≥ 1 Gbps, IP tĩnh" là yêu cầu tối thiểu phi thực tế cho một VPS. RAM dev 8 GB chưa tính Next.js dev server và IDE.
- **Phụ trách:** Chung.

---

## F. Công nghệ lỗi thời hoặc có rủi ro (tại thời điểm 09/2026)

### F-01 · Nghiêm trọng · MinIO bản cộng đồng đã ngừng phát hành
- **Vị trí:** bìa; 2.1.2; 2.4.1 ("MinIO latest stable"); 2.6.2 ("RELEASE.2024+"); 6.5 (`minio/minio:latest`).
- **Nội dung:** MinIO gỡ admin console khỏi bản cộng đồng (05/2025), ngừng phát hành binary và Docker image (10/2025), chuyển sang chế độ bảo trì (12/2025), archive repository (02/2026); theo báo cáo cộng đồng, image `minio/minio` đã bị gỡ khỏi Docker Hub (11/09/2026). → `docker compose pull` theo 6.5 có thể **thất bại ngay**; "latest stable" không còn ý nghĩa; không còn bản vá bảo mật.
- **Phụ trách:** TV3, Chung.

### F-02 · Cao · Danh sách trình duyệt hỗ trợ mâu thuẫn và không khớp với framework
- **Vị trí:** 2.4.3 (tr. 14) ↔ 5.4 (tr. 49).
- **Nội dung:** 2.4.3: Chrome 90+, Firefox 88+, Safari 14+, Edge 90+ ↔ 5.4: Chrome 112+, Firefox 113+, Safari 16+, Edge 112+. Next.js 16 mặc định chỉ hỗ trợ Chrome/Edge/Firefox 111+ và Safari 16.4+; Tailwind CSS v4 cần Safari 16.4+, Chrome 111+, Firefox 128+ → **cả hai** danh sách trong SRS đều không đúng với stack hiện tại.
- **Phụ trách:** Chung (frontend).

### F-03 · Cao · Phiên bản Next.js/Auth.js không thống nhất và có rủi ro
- **Vị trí:** bìa ("Next.js App Router"); 1.4 mục 10 ("Next.js **15**"); 6.1 ("Next.js **14+**").
- **Nội dung:** ba cách ghi phiên bản khác nhau, trong khi bản chính hiện tại là Next.js 16. Auth.js (NextAuth) v5 **vẫn là beta**; dự án hiện do đội Better Auth duy trì (chủ yếu vá bảo mật), và chính họ khuyến nghị dùng Better Auth cho dự án mới.
- **Phụ trách:** Chung, TV1.

### F-04 · Cao · Công cụ phát triển được liệt kê không hỗ trợ .NET 10; Node 20 đã hết hỗ trợ
- **Vị trí:** 2.4.1, 2.4.2 (tr. 13–14).
- **Nội dung:** "Visual Studio 2022 v17.12+" **không hỗ trợ .NET 10** — Visual Studio 2026 (18.x) là bản đầu tiên hỗ trợ; "Rider 2024+" cũng quá cũ (cần bản có hỗ trợ .NET 10). "Node.js 20 LTS" là bản tối thiểu, nhưng Node 20 đã hết hỗ trợ ngày 30/04/2026, tức **trước** ngày phát hành SRS (04/06/2026).
- **Phụ trách:** Chung.

### F-05 · Trung bình · MediatR ≥ 13 đã chuyển sang giấy phép thương mại
- **Vị trí:** CONS-002 (tr. 14); 6.3.
- **Nội dung:** SRS bắt buộc dùng MediatR nhưng không ghi phiên bản. Từ 13.0.0, MediatR dùng giấy phép thương mại/RPL-1.5; có Community License miễn phí (mục đích giáo dục đủ điều kiện), nếu không có license key thì thư viện ghi log cảnh báo. Bản ≤ 12.x vẫn theo MIT/Apache 2.0.
- **Phụ trách:** Chung.

### F-06 · Thấp · Trích dẫn tiêu chuẩn đã bị thay thế
- **Vị trí:** bìa, 1.1, 1.4, CONS-005, Phụ lục C.
- **Nội dung:** RFC 7807 → đã bị **RFC 9457** thay thế (07/2023); RFC 7231 → **RFC 9110**; IEEE 830-1998 đã được ISO/IEC/IEEE 29148 thay thế (SRS lại tuyên bố tuân thủ đồng thời cả hai).
- **Phụ trách:** Chung.

---

## G. Thiếu sót yêu cầu

### G-01 · Cao · Không có quên/đặt lại/đổi mật khẩu và xác nhận email
- **Vị trí:** 3.1 (tr. 17–23).
- **Nội dung:** có đăng nhập bằng mật khẩu (FR-AUTH-002) và khóa tài khoản sau 5 lần sai, nhưng **không có** quên/đặt lại mật khẩu hay đổi mật khẩu. Không có xác nhận email, dù `emailConfirmed`, policy `VerifiedAuthor` và "link kích hoạt" đều được nhắc tới.
- **Phụ trách:** TV1.

### G-02 · Trung bình · Không có chức năng quản trị người dùng
- **Vị trí:** 2.3; 7.7; Phụ lục B.
- **Nội dung:** Admin chỉ được tạo bằng seed; không có FR để ban/mở khóa (`IsActive`), gán role, hay xem danh sách người dùng. Tài khoản Admin seed lấy mật khẩu/secret từ đâu cũng không nói.
- **Phụ trách:** TV1.

### G-03 · Trung bình · Thiếu endpoint cho các màn hình đã liệt kê
- **Vị trí:** 5.1 (tr. 46–47); Chương 8.
- **Nội dung:** `/dashboard/recipes` cần danh sách "bài của tôi" có lọc theo trạng thái; `/dashboard/recipes/[id]/edit` cần `GET /recipes/{id}`; không có unarchive; không có API sắp xếp lại bước/nguyên liệu/ảnh (dù có `OrderIndex`); `/dashboard` "tổng quan" không định nghĩa hiển thị gì; form "multi-step wizard" nhưng API tạo recipe chỉ có một request.
- **Phụ trách:** TV2, TV3.

### G-04 · Trung bình · Thiếu ma trận truy vết, sơ đồ trạng thái và tiêu chí chấp nhận
- **Vị trí:** toàn tài liệu.
- **Nội dung:** không có ma trận FR ↔ API ↔ màn hình ↔ test; không có sơ đồ use case hay sơ đồ trạng thái Recipe; không có wireframe; không định nghĩa trang lỗi 404/500 hay các "trang tĩnh" mà sitemap nhắc tới.
- **Phụ trách:** Chung.

### G-05 · Trung bình · Thiếu chính sách lưu giữ và sao lưu dữ liệu
- **Vị trí:** NFR-REL-003 (tr. 43); 7.8.
- **Nội dung:** không dọn refresh token hết hạn và job Hangfire cũ; chỉ backup PostgreSQL (`pg_dump` "03:00 AM" không ghi múi giờ), **không backup MinIO**, không có quy trình khôi phục; không nói tài khoản Admin seed được tạo thế nào.
- **Phụ trách:** Chung, TV4.

### G-06 · Trung bình · Không định nghĩa định dạng nội dung (plain text / Markdown / HTML)
- **Vị trí:** 7.2 `Instructions` "dạng markdown"; FR-RCP-003/010; NFR-SEC-004 "Input sanitization".
- **Nội dung:** Description, Step.Description, Notes là plain text, Markdown hay HTML? Không có câu trả lời thì không thể định nghĩa cách chống XSS (sanitize đầu vào hay escape khi hiển thị) cũng như cách render.
- **Phụ trách:** TV2.

### G-07 · Trung bình · Chưa định nghĩa domain và cách Nginx định tuyến
- **Vị trí:** 5.2 (tr. 47); NFR-SEC-005 (tr. 41); 6.1, 6.5.
- **Nội dung:** production dùng subdomain `api.culinaryblog.com` (5.2) hay `domain.com` (NFR-SEC-005)? Nginx định tuyến `/api`, `/hangfire`, `/scalar`, `/health`, ảnh MinIO ra sao? Chứng chỉ HTTPS lấy từ đâu (Let's Encrypt?) — đều chưa có.
- **Phụ trách:** Chung.

### G-08 · Thấp · Thiếu đặc tả xác thực phía frontend
- **Vị trí:** 5.1; 6.1.
- **Nội dung:** không mô tả cách bảo vệ route (middleware), luồng tự refresh khi gặp 401, cách xử lý 403, hay cách đồng bộ phiên giữa nhiều tab — trong khi `BangPhanCong.docx` giao cho TV1 "xử lý lưu trữ token an toàn và tự động refresh token khi nhận lỗi 401".
- **Phụ trách:** TV1.

---

## Ma trận theo thành viên

| Thành viên | Số lỗi liên quan | Mã lỗi (\* = Nghiêm trọng) |
|---|---|---|
| TV1 — Auth & Profile, FR-JOB-001 | 23 | A-02, B-08, B-10, B-11, C-01*, C-02, C-03*, D-05*, D-06, D-07, D-08, D-09, D-10, D-13, D-16, D-25, D-26, D-27, D-30, F-03, G-01, G-02, G-08 |
| TV2 — Recipe Core | 19 | B-01*, B-02, B-03, B-04, B-05, B-08, B-10, C-07, C-09, C-11, D-01*, D-03*, D-04*, D-20, D-21, D-28, D-29, G-03, G-06 |
| TV3 — Categories, File/MinIO, Image | 17 | A-02, B-01*, B-02, B-04, B-05, B-06, B-07, B-10, C-08, D-03*, D-04*, D-14, D-15, D-16, D-22, F-01*, G-03 |
| TV4 — Search, SEO, Observability | 22 | A-02, B-02, B-04, B-05, B-08, B-10, C-04, C-11, D-02*, D-03*, D-04*, D-11, D-12, D-16, D-17, D-23, D-24, D-31, E-01, E-02, E-04, G-05 |
| Chung — nền tảng (cả nhóm) | 35 | A-01, A-03, A-04, A-05, A-06, A-07, A-08, B-01*, B-09, C-04, C-05, C-06, C-07, C-10, D-01*, D-04*, D-10, D-12, D-13, D-18, D-19, D-20, D-32, E-03, E-05, E-06, F-01*, F-02, F-03, F-04, F-05, F-06, G-04, G-05, G-07 |

Một lỗi có thể liên quan đến nhiều thành viên.

---

## Nguồn tham khảo cho các nhận định kỹ thuật

- Npgsql — Concurrency Tokens (`xmin`): https://www.npgsql.org/efcore/modeling/concurrency.html
- Next.js — Supported Browsers (v16): https://nextjs.org/docs/architecture/supported-browsers
- Google Search Central — Sitemaps ping endpoint is going away (06/2023): https://developers.google.com/search/blog/2023/06/sitemaps-lastmod-ping
- Dòng thời gian ngừng phát triển MinIO bản cộng đồng: https://blog.vonng.com/en/db/minio-resurrect/ ; báo cáo image bị gỡ khỏi Docker Hub: https://github.com/cloudmarktplaats/cloudmarktplaats/issues/48
- Lucky Penny Software — MediatR Licensing FAQ: https://luckypennysoftware.com/faq
- Microsoft Q&A — Visual Studio 2022 và .NET 10: https://learn.microsoft.com/en-us/answers/questions/5880971/vss-web-visual-studio-2022-support-for-net-develop
- Auth.js gia nhập Better Auth (tình trạng v5): https://github.com/nextauthjs/next-auth/discussions/13252
- Node.js 20 EOL (30/04/2026): https://eolradar.com/node-js-20-end-of-life-2026/
- Seq — Getting started with Docker (`ACCEPT_EULA`): https://datalust.co/docs/getting-started-with-docker
- Mailpit thay thế MailHog: https://sendpigeon.dev/blog/mailhog-alternatives

