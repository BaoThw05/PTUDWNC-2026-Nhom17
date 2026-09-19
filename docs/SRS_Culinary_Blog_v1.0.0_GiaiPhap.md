# Phân tích giải pháp & hướng đi — SRS Culinary Blog v1.0.0

> **Đầu vào:** `SRS_Culinary_Blog_v1.0.0_DanhSachLoi.md` (82 lỗi, mã A-01 → G-08).
> **Cách đọc:** Phần 1 là quyết định về chính tài liệu SRS. Phần 2 (quyết định chặn) và Phần 3 (mức Cao) phân tích từng nhóm vấn đề: mỗi nhóm có các phương án, bảng ưu/nhược điểm và **khuyến nghị kèm lý do**. Phần 4 là bảng sửa nhanh cho các lỗi không cần cân nhắc phương án. Phần 5 là **hướng đi đề xuất** (tổng hợp quyết định, lộ trình, phân công, câu hỏi cần hỏi giảng viên). Phụ lục đối chiếu mã lỗi ↔ giải pháp để đảm bảo không lỗi nào bị bỏ sót.
>
> Mọi khuyến nghị ở đây là **đề xuất để nhóm thảo luận**, không phải quyết định đã chốt.

## Bối cảnh dùng để đánh giá phương án

- Nhóm 4 sinh viên, làm trong một học kỳ; kinh nghiệm web còn chưa nhiều; phát triển trên Windows.
- SRS là **tài liệu giáo trình** (bìa ghi "Giáo trình Phát triển Ứng dụng Web Nâng cao — Phiên bản V4"; đối tượng đọc gồm "Giảng viên và Sinh viên"). Vì vậy **mọi thay đổi về phạm vi nên được giảng viên xác nhận**; nhóm chỉ tự quyết những gì thuộc về *làm rõ* hoặc *sửa lỗi kỹ thuật*.
- Stack đã phân công trong `BangPhanCong.docx`: .NET 10 Clean Architecture + MediatR, Next.js App Router + Tailwind + TanStack Query + Auth.js v5, PostgreSQL, Redis, MinIO, Hangfire.

**Tiêu chí chấm phương án** (dùng trong các bảng bên dưới):

| Ký hiệu | Tiêu chí |
|---|---|
| **Đúng** | Đúng kỹ thuật, an toàn, không tạo lỗi mới |
| **Khớp** | Ít phải thay đổi SRS, ít phải xin giảng viên |
| **Vừa sức** | Độ phức tạp phù hợp nhóm sinh viên, hoàn thành kịp |
| **Song song** | Giúp 4 người làm song song, ít vỡ khi ghép |
| **Học thuật** | Thể hiện được kiến thức "web nâng cao" mà môn học nhắm tới |

---

## Phần 1. Xử lý chính tài liệu SRS

### S-01 · Sửa SRS theo cách nào? (A-01 → A-08, E-03)

| Phương án | Ưu điểm | Nhược điểm |
|---|---|---|
| **A.** Sửa thẳng file Word/PDF thành v1.1 | Một tài liệu duy nhất, giống bản gốc | Khó review thay đổi; khó cộng tác 4 người; PDF gốc đã mất chữ (A-07) |
| **B.** Giữ nguyên v1.0, viết thêm tài liệu *Errata* (bản làm rõ) + ADR (Architecture Decision Record) cho từng quyết định | Không đụng vào tài liệu giáo trình; minh bạch "đã đổi gì, vì sao"; đáp ứng NFR-MAINT-003 (ADR) | Người đọc phải đọc 2 tài liệu; dễ có người vẫn code theo v1.0 |
| **C.** Đưa `SRS_Culinary_Blog_v1.0.0.md` vào repo làm nguồn sự thật, sửa qua Pull Request; phần API lấy **OpenAPI** làm hợp đồng | Có lịch sử diff, review theo PR; API contract được sinh/kiểm tra từ code (Scalar đã có trong SRS) nên không thể lệch nữa | Cần kỷ luật: đổi code thì đổi tài liệu; phải thống nhất quy trình PR |

**Khuyến nghị: C + B.** Đưa bản Markdown vào `docs/` của repo, tạo **SRS v1.1 (bản làm rõ của nhóm)** kèm bảng Change Request ở đầu file (mã lỗi → thay đổi → trạng thái "Nhóm tự quyết / Chờ GV xác nhận / Đã xác nhận"). Mỗi quyết định lớn (S-03 → S-18) ghi thành một ADR ngắn trong `docs/adr/`. Chương 8 không viết tay nữa mà trỏ tới OpenAPI sinh từ code.
*Lý do:* các lỗi của SRS phần lớn sinh ra từ việc một quyết định bị chép lại ở 3–5 chỗ (xem "Nguyên nhân gốc" trong danh sách lỗi). Cách C cắt tận gốc chuyện đó: hợp đồng API chỉ còn một nguồn.

### S-02 · Khi các chương mâu thuẫn, chương nào thắng?

| Phương án | Ưu điểm | Nhược điểm |
|---|---|---|
| **A.** Chương 3 (FR) luôn thắng | Chương chi tiết nhất về hành vi; phân công đang bám theo mã FR | Chương 3 cũng có lỗi kỹ thuật (D-01, D-02, D-05) |
| **B.** Chương 7–8 luôn thắng | Gần với code (schema, endpoint) | Chương 8 bị cắt chữ, thiếu cột; Chương 7 mâu thuẫn với FR về soft delete |
| **C.** Quy tắc phân tầng + ngoại lệ được ghi rõ | Mỗi loại quyết định lấy từ chương có thẩm quyền nhất; ngoại lệ đi qua ADR | Phải thuộc quy tắc |

**Khuyến nghị: C**, theo thứ tự:
1. **Bảo mật và tính đúng kỹ thuật** thắng mọi chương (ví dụ: S-05 thắng FR-AUTH-003; S-04 thắng 7.1).
2. **Phạm vi** (1.2) và **hành vi/quy tắc nghiệp vụ** lấy theo Chương 3.
3. **Kiểu dữ liệu, độ dài, ràng buộc** lấy theo Chương 7 *khi Chương 3 không nói*; nếu cả hai cùng nói thì chọn theo S-10.
4. **Chương 8, Phụ lục A/B** không có thẩm quyền: sẽ được viết lại theo quyết định cuối cùng (OpenAPI).
5. **NFR** áp dụng theo bản "đồ án" (S-16).

---

## Phần 2. Các quyết định chặn — phải chốt trước khi code

### S-03 · Xóa dữ liệu: hard delete hay soft delete? (B-01, B-07, D-21)

| Phương án | Ưu điểm | Nhược điểm |
|---|---|---|
| **A. Hard delete toàn bộ** (bỏ `IsDeleted` khỏi BaseEntity) | Đúng FR-RCP-007 và FR-CAT-005 (nơi mô tả chi tiết nhất, có chủ ý: "không dùng soft delete pattern cho recipe"); `CASCADE`/`RESTRICT` chạy tự nhiên; unique index đơn giản; dọn MinIO nhất quán; không cần Global Query Filter | Phải sửa NFR-REL-003, Chương 7, 8.2/8.3, Phụ lục A/C; không khôi phục được bản ghi đã xóa (bù bằng Archive + backup) |
| **B. Soft delete toàn bộ** | Khớp Chương 7, 8 và NFR-REL-003; khôi phục được | Mọi `UNIQUE` phải chuyển thành partial index `WHERE "IsDeleted" = false`; phải tự code cascade; không được xóa ảnh MinIO ngay (cần job dọn); FR-CAT-005 và FK `RESTRICT` vướng bản ghi đã xóa mềm; dễ quên filter khi Include; đánh số lại step càng khó (D-21); không có FR khôi phục |
| **C. Lai:** Recipe soft delete (thùng rác N ngày, job dọn định kỳ xóa thật + xóa MinIO); bảng con và Category hard delete | Khôi phục được thứ quan trọng nhất; bảng con vẫn đơn giản | Thêm job dọn, thêm endpoint khôi phục (SRS chưa có); vẫn cần partial unique index cho `Recipe.Slug`; vẫn phải sửa FR-RCP-007 |

**Khuyến nghị: A.** SRS đã có trạng thái **Archived** đóng vai trò "ẩn mà không mất" (FR-RCP-006), nên nhu cầu giữ dữ liệu đã được đáp ứng; soft delete ở đây chỉ thêm độ phức tạp mà không mang lại chức năng mới. Nếu giảng viên bắt buộc có soft delete thì chọn **C**, không chọn B.
**Sửa SRS:** bỏ `IsDeleted` ở 6.4, 7.1, 7.2; bỏ dòng soft delete ở NFR-REL-003; bỏ "(soft delete)" ở 8.2/8.3; Phụ lục A dòng 404 bỏ "đã soft-delete"; Phụ lục C bỏ mục "Soft Delete". FR-CAT-005 đếm recipe ở **mọi** trạng thái và đổi "soft constraint" thành "ràng buộc nghiệp vụ bắt buộc".
-*dev: chốt phương án C.

### S-04 · Kiểm soát cập nhật đồng thời (D-01, D-28, C-07)

**Chọn cơ chế lưu "phiên bản" của dòng dữ liệu:**

| Phương án | Ưu điểm | Nhược điểm |
|---|---|---|
| **A.** Cột hệ thống `xmin` của PostgreSQL ánh xạ sang `uint Version` với `[Timestamp]` | Cách Npgsql khuyến nghị; không cần thêm cột; tự thay đổi mỗi lần UPDATE | Chỉ có trên PostgreSQL; sửa bảng con không làm đổi `xmin` của Recipe |
| **B.** Cột `Version` (`long`/`Guid`) do `AuditInterceptor` tự tăng, cấu hình `IsConcurrencyToken()` | Không phụ thuộc DB; dễ hiểu | Tự viết code; quên tăng là mất tác dụng |
| **C.** Bỏ optimistic concurrency ("ghi sau thắng") | Đơn giản nhất | Mất FR-RCP-004 (Must Have) và mất một điểm kiến thức của môn |

**Chọn cách client gửi phiên bản lên:**

| Phương án | Ưu điểm | Nhược điểm |
|---|---|---|
| **T1.** Header `ETag`/`If-Match` → 412 khi lệch, 428 khi thiếu | Đúng chuẩn HTTP | Phải thêm `If-Match` vào CORS Allow-Headers và `ETag` vào Expose-Headers; lệch với 409 trong FR; frontend phức tạp hơn |
| **T2.** Trường `version` trong body PUT → **409** khi lệch | Đúng FR-RCP-004 ("hoặc trong request body") và A2 (409); không phải sửa CORS; dễ làm với Axios/TanStack Query | Kém "chuẩn REST" hơn T1 |

**Khuyến nghị: A + T2.** `GET` trả kèm `version`; `PUT /recipes/{id}` bắt buộc gửi `version`. Khi thêm/sửa/xóa step, ingredient, image thì cập nhật `Recipe.UpdatedAt` trong cùng transaction → dòng Recipe bị UPDATE → `xmin` đổi → xử lý được D-28. Phụ lục A/B đổi `RECIPE_CONCURRENCY_CONFLICT` thành **409**. Sửa kịch bản ở 3.3 thành "chủ bài và Admin, hoặc cùng một người trên hai tab".

### S-05 · Kiến trúc xác thực Next.js ↔ .NET và nơi lưu token (C-01, C-02, C-03, D-05, D-06, D-08, D-09, F-03, G-08) — *module TV1*

| Phương án | Mô tả ngắn |
|---|---|
| **A. SPA thuần** | Frontend giữ access token trong bộ nhớ, refresh token trong localStorage, gọi thẳng API (đúng như 5.2). |
| **B. Cookie HttpOnly từ backend** | Backend đặt refresh token trong cookie `HttpOnly; Secure; SameSite=Lax; Path=/api/v1/auth`; access token trả trong body và giữ trong bộ nhớ. |
| **C. BFF với Auth.js v5** | Auth.js giữ phiên trong cookie HttpOnly đã mã hóa, bên trong cất access/refresh token của backend. Credentials provider gọi `/auth/login`; Google provider lấy `id_token` rồi server Next.js gọi `POST /auth/google { idToken }`; refresh trong callback `jwt`; logout gọi `/auth/logout` rồi `signOut()`. |
| **D. Backend tự làm Google OAuth** | Dùng Google handler của ASP.NET, callback `/api/v1/auth/google/callback` (đúng như 5.3), sau đó redirect về frontend kèm mã dùng một lần. |
| **E. Better Auth** | Thay Auth.js bằng Better Auth ở frontend. |

| Tiêu chí | A | B | C | D | E |
|---|---|---|---|---|---|
| Đúng / bảo mật | Yếu — XSS lấy được refresh token 7 ngày | Tốt | Tốt | Tốt | Tốt |
| Khớp SRS & phân công | Khớp 5.2, lệch 6.1 | Lệch 5.2 ("không dùng cookie") | **Khớp 6.1, FR-AUTH-003, BangPhanCong** | Khớp 5.3, bỏ Auth.js | Lệch nhiều nhất: trùng vai trò với ASP.NET Identity |
| Vừa sức | Dễ nhất | Trung bình (CORS credentials, CSRF) | Trung bình–khó (callback `jwt`, refresh) | Khó (tự làm state, PKCE, đổi mã) | Khó (hai hệ thống user) |
| SSR / bảo vệ route | Không (dashboard phải CSR hoàn toàn) | Hạn chế (cookie gắn domain API) | **Có** (middleware/Server Component đọc được phiên) | Tùy cách làm | Có |
| Rủi ro thư viện | Không | Không | Auth.js v5 vẫn beta | Không | Thư viện trẻ, ít ví dụ kết hợp .NET |

**Khuyến nghị: C**, kèm các quyết định con:
1. **Google:** backend **chỉ nhận `{ idToken }`** và tự xác minh (ví dụ `GoogleJsonWebSignature.ValidateAsync` của `Google.Apis.Auth`, với `Audience` = ClientId), sau đó kiểm tra `email_verified`. Chỉ tự liên kết với tài khoản local khi email đã xác minh. `ClientSecret` chỉ nằm ở Next.js; backend chỉ cần `ClientId`. User mới tạo qua Google được sinh `userName` tự động và cũng nhận email chào mừng. → xử lý C-03, D-05.
2. **Đăng ký:** giữ auto-login (FR-AUTH-001): gọi `/auth/register` xong thì `signIn("credentials")`. Không bắt buộc `userName` khi đăng ký (Identity `UserName` = email) → bớt một lỗi trùng, đồng nhất với luồng Google. → C-01, D-25.
3. **Access token phía client:** giai đoạn đầu đưa `accessToken` (hạn 15 phút) vào session để TanStack Query dùng — đơn giản, rủi ro chấp nhận được, ghi vào ADR. Khi còn thời gian thì chuyển sang gọi qua Route Handler proxy để token không bao giờ xuống trình duyệt.
4. **Đăng nhập local:** dùng `SignInManager.CheckPasswordSignInAsync(user, password, lockoutOnFailure: true)` (tự đếm số lần sai và tự khóa); kiểm tra `IsActive` → 403 `AUTH_ACCOUNT_DISABLED`; bỏ điều kiện "LockoutEnabled = false". Chấp nhận để 409 (email đã tồn tại) và 423 (tài khoản bị khóa) vì dễ dùng hơn, đồng thời hạn chế việc dò tài khoản bằng rate limit (S-15) — ghi rõ đây là rủi ro chấp nhận. → D-06.
5. **Logout:** endpoint `AllowAnonymous`, chỉ cần `refreshToken` trong body (thu hồi nếu hash khớp); nếu có kèm access token hợp lệ thì kiểm tra token đó thuộc cùng user. → D-09.
6. **Phương án dự phòng:** làm *spike* 1–2 ngày (Auth.js v5 + Credentials + Google + refresh). Nếu gặp vướng không gỡ được thì chuyển sang **B** — backend giữ nguyên, chỉ đổi phía frontend.

### S-06 · Refresh token: xoay vòng, phát hiện dùng lại, lưu trữ (D-07)

| Phương án | Ưu điểm | Nhược điểm |
|---|---|---|
| **A.** Xoay vòng nghiêm ngặt: token đã bị thay thế mà dùng lại → thu hồi **cả family** ngay (NFR-SEC-002) | An toàn nhất | Đăng xuất nhầm khi hai request refresh chạy song song (nhiều tab, callback `jwt` của Auth.js) |
| **B.** Như A nhưng có **thời gian ân hạn** ngắn (ví dụ 30 giây): token vừa bị thay thế trong 30 giây được dùng lại thì cấp cặp mới, *không* thu hồi family | Tránh đăng xuất nhầm; vẫn bắt được việc dùng lại token cũ | Phức tạp hơn một chút; có "cửa sổ" 30 giây |
| **C.** Không xoay vòng, refresh token dùng tới khi hết hạn | Đơn giản, không có race | Trái FR-AUTH-004 (Must) và NFR-SEC-002; token lộ ra thì dùng được 7 ngày |
| **D.** Chỉ khóa (serialize) refresh ở phía client | Không phải đổi backend | Không đảm bảo khi có nhiều tab hoặc nhiều tiến trình server |

**Khuyến nghị: B.** Thiết kế thống nhất:
- Token: 32 byte ngẫu nhiên (`RandomNumberGenerator`), mã hóa base64url; **chỉ lưu SHA-256 hex** (64 ký tự, khớp `varchar(64)` ở 7.8). *Chốt một con số duy nhất* — 128-bit (NFR-SEC-002) cũng đủ an toàn; quan trọng là bỏ con số 512-bit ở FR-AUTH-001.
- Bảng `RefreshTokens`: giữ các cột của 7.8, **thêm** `FamilyId uuid` và `RevokedReason` (`Rotated` / `Logout` / `ReuseDetected` / `Expired`); bỏ `IsRevoked` (dùng `RevokedAt IS NOT NULL`).
- Luồng refresh: token hợp lệ → thu hồi (`Rotated`), cấp token mới cùng `FamilyId`. Token đã `Rotated` mà dùng lại: nếu trong 30 giây → cấp mới; nếu quá 30 giây → thu hồi cả family (`ReuseDetected`), ghi log cảnh báo bảo mật, trả 401 `AUTH_REFRESH_TOKEN_REVOKED`. Token đã `Logout` mà dùng lại → 401, không thu hồi family.
- Thêm Hangfire recurring job dọn token đã hết hạn quá 30 ngày (xử lý luôn G-05).

### S-07 · Chiến lược cache (D-03, D-04)

| Phương án | Ưu điểm | Nhược điểm |
|---|---|---|
| **A.** Redis cache-aside trong MediatR `CachingBehavior` (theo 6.3, NFR-PERF-003) | Kiểm soát key/TTL ở tầng Application; dùng được cho mọi query; khớp NFR-SCALE (Redis) | Tự viết serialize, key, invalidation; phải đưa quyền người xem vào key để không lộ Draft; nhiều code, dễ sai |
| **B.** **ASP.NET Core Output Caching** với Redis store (`Microsoft.AspNetCore.OutputCaching.StackExchangeRedis`) cho các GET công khai, xóa theo tag | Rất ít code; policy mặc định **không cache request có xác thực** → không lộ Draft; có sẵn xóa theo tag; khớp FR-RCP-001/002 | Cache nằm ở tầng HTTP → handler muốn xóa cache phải đi qua interface (`ICacheInvalidator`) hiện thực ở tầng ngoài; người đã đăng nhập không hưởng cache (chấp nhận được vì 2.3 nói Guest là "đại đa số") |
| **C.** `HybridCache` (.NET 9+) trong handler | Kết hợp bộ nhớ + Redis, chống cache stampede, API gọn | Vẫn là cache-aside (phải tự xử lý key theo quyền); cần kiểm tra khả năng xóa theo tag ở phiên bản dùng; ít ví dụ cho người mới |
| **D.** Chưa cache ở backend; làm đúng chức năng trước, đo rồi mới bật | Đơn giản nhất, không có bug dữ liệu cũ; ≤ 10.000 recipe có index thì PostgreSQL vẫn đủ nhanh | Chưa đáp ứng NFR-PERF-003; Redis trong SRS chưa được dùng |

**Khuyến nghị: B làm cơ chế cache duy nhất, triển khai theo thứ tự của D** (giai đoạn 2 làm đúng chức năng, giai đoạn 3 mới bật cache).
- **Bỏ** `IMemoryCache` (FR-CAT-001) và `CachingBehavior`/`CacheInvalidationBehavior` (6.3) khỏi SRS.
- **Bảng TTL duy nhất** (lấy số của NFR-PERF-003 vì TTL ngắn + xóa theo tag thì ít dữ liệu cũ): danh mục 30 phút · danh sách recipe 5 phút · chi tiết recipe 5 phút · tìm kiếm 1 phút. Dùng absolute expiration, **không dùng sliding**.
- **Tag:** `categories`, `recipes`, `recipe:{id}`. Command thành công thì xóa tag tương ứng; publish/unpublish/archive/delete recipe xóa **cả** `categories` (vì `recipeCount` đổi).
- Endpoint công khai chỉ trả Published (S-11) nên cache được an toàn; dữ liệu cá nhân (bài của tôi, Draft) đi qua endpoint riêng, không cache.
- Next.js: `/recipes/[slug]` ISR `revalidate=300` là đủ; *tùy chọn* thêm webhook gọi `revalidateTag` khi publish. Trang dashboard dùng CSR, không ISR.
- Bộ đếm rate limit dùng bộ nhớ trong tiến trình (một instance) — sửa câu "Rate limiting counters" ở 6.1.

### S-08 · Tìm kiếm toàn văn tiếng Việt (D-02, D-23, D-24)

| Phương án | Ưu điểm | Nhược điểm |
|---|---|---|
| **A.** Full-text search: tự tạo text search configuration (sao chép `simple` + dictionary `unaccent`), cột `tsvector` cập nhật bằng **trigger** (đúng 7.2) + GIN index; truy vấn bằng `plainto_tsquery`/`websearch_to_tsquery` hoặc bộ dựng prefix đã lọc ký tự | Đúng tinh thần FR-SRCH-001 (`tsvector`, `ts_rank`, GIN); nhanh; có xếp hạng | Không chịu được lỗi chính tả; bỏ dấu làm "pho" khớp cả "phố" (chấp nhận được với blog nấu ăn) |
| **B.** `pg_trgm` (similarity/ILIKE) trên cột đã bỏ dấu | Chịu lỗi gõ sai, tìm được chuỗi con; dễ hiểu | Xếp hạng kém với văn bản dài; không dùng `tsvector` như FR yêu cầu |
| **C.** A làm chính; nếu 0 kết quả thì fallback sang trigram trên Title và gợi ý "Có phải bạn muốn tìm…" | Trải nghiệm tốt nhất; dùng đúng **cả hai** extension mà SRS bắt buộc | Thêm một truy vấn và chút logic |
| **D.** Công cụ ngoài (Meilisearch, Elasticsearch) | Tìm kiếm tiếng Việt và chịu lỗi chính tả rất tốt | Thêm service và bài toán đồng bộ dữ liệu; trái CONS-006 ("PostgreSQL là DBMS duy nhất") → **loại** |

**Khuyến nghị: A bắt buộc, C nếu còn thời gian.**
- Dùng trigger (khớp 7.2), hoặc generated column với một hàm bọc `unaccent` khai báo IMMUTABLE — chọn một cách và sửa câu "computed column … trigger" ở FR-SRCH-001.
- Trọng số: `setweight` Title = A, Description = B. Tên nguyên liệu (C) để giai đoạn sau, vì phải cập nhật lại vector khi bảng con thay đổi.
- Từ khóa tìm kiếm cũng phải qua `unaccent`; chỉ giữ chữ và số trước khi dựng prefix `:*` → không còn lỗi cú pháp tsquery.
- **CONS-006** thêm câu: "SQL trong migration (`migrationBuilder.Sql`) được phép cho extension, text search configuration, trigger, index đặc thù".
- Index: `(Status, CreatedAt DESC)`, `CategoryId`, `AuthorId`, `CookTimeMinutes`; sửa NFR-PERF-004 thành "các truy vấn lọc/sắp xếp chính phải có index phù hợp, kiểm chứng bằng `EXPLAIN ANALYZE`" thay vì "mọi cột".
-*dev: Chốt phương án B (Cho phép dùng tiếng Việt không dấu).

### S-09 · Object storage khi MinIO bản cộng đồng đã ngừng (F-01)

| Phương án | Ưu điểm | Nhược điểm |
|---|---|---|
| **A.** Ghim image MinIO cộng đồng cuối cùng còn kéo được (ghim theo digest) | Không đổi code và tài liệu; nhóm đã quen MinIO | Không còn bản vá bảo mật; từ 05/2025 bản cộng đồng chỉ còn trình duyệt object cơ bản (admin console đã bị lược bỏ); image có thể bị gỡ nốt; **không nên** mở ra Internet |
| **B.** Chuyển sang một phần mềm S3-compatible mã nguồn mở khác vẫn còn được bảo trì | Còn được vá lỗi; vẫn dùng `AWSSDK.S3` | Cấu hình khác, ít tài liệu cho .NET; mức hỗ trợ bucket policy/public-read mỗi phần mềm khác nhau → phải thử trước |
| **C.** Dịch vụ cloud tương thích S3 (Cloudflare R2, AWS S3) cho bản demo/production | Không phải vận hành; có sẵn CDN | Cần tài khoản (có thể cần thẻ); phụ thuộc Internet lúc demo; tốn phí nếu vượt free tier |
| **D.** Lưu file local (`LocalFileStorageService` — SRS đã cho phép ở dev) | Không cần container; đơn giản nhất | Không học được S3; không dùng cho production |

**Khuyến nghị:** thiết kế **trung lập với nhà cung cấp** để sau này đổi chỉ bằng cấu hình: `AWSSDK.S3` với `ServiceURL` + `ForcePathStyle`, cấu hình riêng `PublicBaseUrl`, DB **chỉ lưu object key** (S-12). Dev dùng **A** nếu còn kéo được image, không thì dùng **D**. Dành một buổi thử **B** nếu cần giải pháp lâu dài; bản demo trên Internet dùng **C**. Sửa SRS: "Object Storage tương thích S3 (MinIO hoặc tương đương)", bỏ "latest stable"/"RELEASE.2024+". **Cần báo giảng viên** vì bìa SRS ghi rõ MinIO.

### S-10 · Hợp đồng API chung: response, mã lỗi, đặt tên, phân trang (B-08, B-09, C-04, C-05, C-06, C-10)

**(a) Response có vỏ bọc hay không?**

| Phương án | Ưu điểm | Nhược điểm |
|---|---|---|
| **A.** Không vỏ bọc: trả thẳng resource; danh sách trả `PagedResult` | Khớp Chương 3; `TypedResults` gọn; schema OpenAPI rõ; frontend ít phải bóc lớp | Muốn thêm metadata chung thì phải để trong header |
| **B.** `{ data, meta }` cho mọi response (Chương 8) | Đồng nhất, dễ mở rộng | Thêm lớp bọc cho mọi DTO; Chương 3 đang không dùng |

**Khuyến nghị: A** — `PagedResult<T> { items, page, pageSize, totalCount, totalPages, hasNextPage, hasPreviousPage }`.

**(b) Mã status cho lỗi validation**

| Phương án | Ưu điểm | Nhược điểm |
|---|---|---|
| **A.** 400 cho JSON/tham số sai định dạng; **422** cho lỗi FluentValidation và vi phạm quy tắc nghiệp vụ | Khớp khoảng 15 FR và định nghĩa 422 ở Phụ lục A; frontend phân biệt được "gửi sai định dạng" với "dữ liệu không hợp lệ" | Phải map trong exception handler (vài dòng) |
| **B.** 400 cho mọi lỗi | Mặc định của `Results.ValidationProblem` | Phải sửa khoảng 15 FR |

**Khuyến nghị: A.** Bảng mã status chốt:

| Status | Khi nào |
|---|---|
| 200 / 201 / 204 | GET–PUT–PATCH thành công / tạo mới (kèm `Location`) / DELETE, logout |
| 400 | Body/tham số sai định dạng |
| 401 | Thiếu/sai access token; refresh token sai, hết hạn, bị thu hồi |
| 403 | Đã xác thực nhưng không đủ quyền; tài khoản bị vô hiệu hóa |
| 404 | Không tìm thấy (kể cả Draft của người khác — tránh lộ slug) |
| 409 | Trùng dữ liệu duy nhất (email, tên danh mục); xóa danh mục còn recipe; **xung đột phiên bản** |
| 413 / 415 | File quá 5 MB / định dạng không cho phép *(hoặc giữ 400 như Phụ lục B nếu muốn ít thay đổi)* |
| 422 | Validation; vi phạm quy tắc nghiệp vụ (publish thiếu điều kiện…) |
| 423 | Tài khoản đang bị khóa tạm thời |
| 429 | Vượt rate limit (kèm `Retry-After`) |
| 500 / 502 / 503 | Lỗi không lường trước / Google lỗi / DB hoặc storage không khả dụng |

**(c) Định dạng lỗi:** RFC 9457 Problem Details qua `AddProblemDetails()` + `IExceptionHandler`. `type` là URI (hoặc `about:blank`); **thêm trường mở rộng `code`** (`AUTH_EMAIL_EXISTS`…) và `traceId`; `errors` cho validation. Frontend hiển thị thông báo theo `code` (đáp ứng NFR-USE-003); backend không hardcode chuỗi tiếng Việt trong exception. Bổ sung các mã còn thiếu ở C-06 vào Phụ lục B.

**(d) Phân trang và sắp xếp:** `page` ≥ 1; `pageSize` mặc định 12, tối đa 50; `sort=-createdAt` (tiền tố `-` là giảm dần; chỉ cho các trường trong whitelist `createdAt`, `title`, `cookTimeMinutes`, `publishedAt`). Bỏ `sortBy/sortOrder` của Chương 8.

**(e) Quy ước đặt tên:** tên JSON (camelCase) **trùng** tên thuộc tính C#; chọn tên rõ nghĩa nhất.

| Chọn | Thay cho |
|---|---|
| `displayName` | `fullName` |
| `prepTimeMinutes`, `cookTimeMinutes` (cột `PrepTimeMinutes`, `CookTimeMinutes`) | `prepTime`, `cookTime` |
| `durationMinutes` | `timerMinutes` |
| `orderIndex` | `sortOrder` |
| `imageId`, `originalUrl`, `mediumUrl`, `thumbnailUrl` | `url`, `imgId` |
| Policy `AuthorPolicy`, `AdminPolicy` | `"Admin"`, `"VerifiedAuthor"` |
| `IEmailService` | `IEmailSender` |

**(f) Nguồn sự thật cho API:** OpenAPI sinh từ code (`Microsoft.AspNetCore.OpenApi` + Scalar). Ở giai đoạn 0 viết trước các endpoint *stub* trả dữ liệu giả để frontend bắt tay làm ngay; frontend sinh kiểu TypeScript từ OpenAPI (ví dụ `openapi-typescript`) → C-01, C-02, C-07, C-08, C-09 không thể tái diễn.

---

## Phần 3. Các quyết định mức Cao

### S-11 · Vòng đời công thức, điều kiện publish, hiển thị và slug (B-02, B-03, B-04, B-05, B-07, D-29)

**Máy trạng thái đề xuất** (bổ sung vào FR-RCP-005/006):

| Từ | Hành động | Đến | Điều kiện |
|---|---|---|---|
| Draft | `publish` | Published | Đủ điều kiện publish (bên dưới); lần publish đầu tiên set `PublishedAt` |
| Published | `unpublish` | Draft | — (giữ nguyên `PublishedAt` để `datePublished` trong SEO không đổi) |
| Draft, Published | `archive` | Archived | — |
| Archived | `unarchive` *(endpoint mới)* | Draft | — |
| Bất kỳ | `delete` | (xóa thật) | Chủ bài hoặc Admin (S-03) |
| Trùng trạng thái đích | bất kỳ | giữ nguyên | Idempotent, trả 200 |

**Điều kiện publish:**

| Phương án | Ưu điểm | Nhược điểm |
|---|---|---|
| **A.** ≥ 1 bước (FR-RCP-005) | Đơn giản, TV2 làm độc lập được | Công thức không có nguyên liệu vẫn lên trang; SEO không pass |
| **B.** ≥ 1 bước + ≥ 1 nguyên liệu (Phụ lục B) | Hợp lý với một công thức nấu ăn; vẫn độc lập với module ảnh | Chưa đáp ứng Rich Results (Google bắt buộc `image`) |
| **C.** B + ≥ 1 ảnh | Đáp ứng NFR-SEO-001; blog ẩm thực có ảnh thì chất lượng hơn | Publish phụ thuộc module ảnh (TV3) và MinIO; khó test sớm |

**Khuyến nghị: B ngay từ đầu, bật C khi module ảnh xong** thông qua cấu hình `Publishing:RequireImage`. Khi recipe đang Published mà xóa bước/nguyên liệu/ảnh cuối cùng → trả **422** `RECIPE_PUBLISH_INCOMPLETE` (rõ ràng, dễ hiểu hơn so với tự động unpublish).

**Quy tắc hiển thị:**

| Phương án | Ưu điểm | Nhược điểm |
|---|---|---|
| **A.** Một endpoint, lọc theo vai trò (như SRS) | Ít endpoint | Không cache được; dễ lộ Draft (D-03); trộn "trang công khai" với "bài của tôi" |
| **B.** Tách: endpoint công khai **chỉ Published** cho mọi người (kể cả Admin); endpoint riêng cho dữ liệu cá nhân | Cache an toàn (S-07); quyền rõ ràng; khớp màn hình dashboard | Thêm 2–3 endpoint |

**Khuyến nghị: B.** Công khai: `GET /recipes`, `GET /recipes/{slug}`, `GET /recipes/search`, `GET /categories/{slug}` (chỉ Published; Draft của người khác trả 404). Cá nhân: `GET /me/recipes?status=` (chủ bài) và `GET /recipes/{id:guid}` (chủ bài/Admin, mọi trạng thái — dùng cho trang sửa). *Tùy chọn:* `GET /admin/recipes`. Kiểm tra quyền sở hữu áp dụng cho **mọi** thao tác ghi (sửa NFR-SEC-006).

**Slug:**

| Phương án | Ưu điểm | Nhược điểm |
|---|---|---|
| **A.** Tự thêm hậu tố `-2`, `-3`… cho cả Category và Recipe; không bao giờ trả 409 vì slug | Khớp FR-CAT-003 và 7.2 ("có thể trùng title khác nhau slug"); người dùng không bị chặn vô lý | Có slug kiểu `pho-bo-3` |
| **B.** Trả 409 để người dùng tự đổi tiêu đề | Slug "đẹp" | Chặn trường hợp trùng tiêu đề hợp lệ giữa hai tác giả |

**Khuyến nghị: A.** Slug được sinh lại khi đổi tiêu đề **chỉ khi recipe chưa từng publish** (`PublishedAt IS NULL`); đã publish thì slug bất biến → **bỏ** yêu cầu redirect 301 và không cần bảng lịch sử slug (sửa NFR-SEO-004). Hàm slugify tiếng Việt phải tự đổi `đ/Đ → d` (chuẩn hóa Unicode FormD chỉ bỏ được dấu, không đổi được `đ`). Xóa `RECIPE_SLUG_EXISTS` khỏi Phụ lục B.

**Danh mục:** FR-CAT-003/004 thêm bước kiểm tra trùng `Name` (so sánh không phân biệt hoa/thường) → 409 `CATEGORY_NAME_EXISTS`; thống nhất độ dài 2–100 ký tự; FR-CAT-005 đếm recipe ở mọi trạng thái. Chức năng chuyển recipe hàng loạt để ở mức Could.

### S-12 · Upload, kiểm tra và xử lý ảnh (C-08, D-14, D-15, D-22)

**Cách upload:**

| Phương án | Ưu điểm | Nhược điểm |
|---|---|---|
| **A.** Upload qua API (multipart), server kiểm tra xong mới đẩy lên storage (luồng chính của SRS) | Kiểm soát toàn bộ việc validation; đơn giản | API chịu băng thông (file ≤ 5 MB nên chấp nhận được) |
| **B.** Presigned URL, trình duyệt upload thẳng lên storage, sau đó có job kiểm tra lại | Giảm tải cho API | File chưa kiểm tra đã nằm trên storage; cần job kiểm tra + xóa; phức tạp hơn nhiều |

**Khuyến nghị: A**; bỏ dòng presigned upload ở 5.3 (hoặc ghi "định hướng").

**Kiểm tra định dạng:**

| Phương án | Ưu điểm | Nhược điểm |
|---|---|---|
| **A.** Tự kiểm tra chữ ký file (magic bytes) cho từng định dạng | Nhẹ, không cần thư viện | Không phát hiện được file "đa hình" (vừa là ảnh vừa chứa nội dung khác) |
| **B.** Dùng thư viện ảnh để decode/identify | Chắc chắn đây là ảnh thật, lấy luôn kích thước | Tốn CPU; mức hỗ trợ AVIF khác nhau tùy thư viện |

**Khuyến nghị: A khi upload, B trong job resize** (decode lỗi → đánh dấu ảnh lỗi). Chữ ký: JPEG `FF D8 FF`; PNG `89 50 4E 47 0D 0A 1A 0A`; WebP byte 0–3 = `RIFF` **và** byte 8–11 = `WEBP`; AVIF byte 4–11 = `ftypavif` hoặc `ftypavis`. `Content-Type` và phần mở rộng **suy ra từ chữ ký**, không lấy từ client. Nếu thư viện resize được chọn không decode được AVIF thì bỏ AVIF khỏi CONS-007 (cần GV đồng ý) — đơn giản hơn là tìm cách hỗ trợ.

**Lưu trữ và phiên bản ảnh:**
- Object key theo thư mục: `recipes/{recipeId}/{imageId}/original.{ext}`, `…/medium.webp`, `…/thumb.webp`, `…/og.jpg`. Xóa ảnh = xóa theo prefix `…/{imageId}/`; xóa recipe = xóa prefix `recipes/{recipeId}/` → hết file mồ côi (D-15).
- DB lưu **key**; URL công khai = `PublicBaseUrl` + key.
- Kích thước: thumbnail 300×300 (crop giữa, cho thẻ danh sách), medium rộng tối đa 800 (giữ tỉ lệ), **og 1200×630** (crop, cho NFR-SEO-002).
- Thư viện: SkiaSharp (MIT) hoặc NetVips (MIT, cần libvips trong container); nếu dùng ImageSharp thì kiểm tra điều khoản giấy phép Six Labors. Vì backend đã tạo sẵn các kích thước, frontend có thể dùng `unoptimized` hoặc custom loader cho `next/image` để không tối ưu ảnh hai lần.
- Ảnh của Draft: chấp nhận public-read vì đường dẫn chứa GUID khó đoán; ghi rõ là rủi ro chấp nhận.
- Ảnh chính: partial unique index `ON "RecipeImages"("RecipeId") WHERE "IsPrimary"`; đổi ảnh chính trong một transaction (bỏ chọn ảnh cũ trước, rồi chọn ảnh mới); ảnh thay thế khi xóa ảnh chính = ảnh có `OrderIndex` nhỏ nhất.
- API theo **8.4** (đầy đủ hơn): upload trả `imageId`; `PATCH /images/{imageId}` cập nhật `altText`, `isPrimary`, `orderIndex`.

### S-13 · Ranh giới Clean Architecture và giấy phép MediatR (D-10, F-05)

| Phương án | Ưu điểm | Nhược điểm |
|---|---|---|
| **A.** Tuân thủ chặt: `ApplicationUser` nằm ở Infrastructure/Identity; Domain không biết Identity (`Recipe.AuthorId` là `string`); Application khai báo các interface `IIdentityService`, `ITokenService`, `IBackgroundJobService`, `IFileStorage` (nhận `Stream` + `contentType` + `fileName`), `ICurrentUser`; có architecture test (NetArchTest/ArchUnitNET) | Đúng CONS-001 và NFR-MAINT-004; test được handler mà không cần hạ tầng; thể hiện được kiến thức kiến trúc | Nhiều interface và boilerplate; lấy tên tác giả phải join trong query/repository |
| **B.** Thực dụng: cho Application tham chiếu Identity và Hangfire; `ApplicationUser` nằm trong Domain | Ít code, bám đúng các luồng FR đang viết | Vi phạm chính ràng buộc SRS đặt ra; architecture test sẽ fail |
| **C.** Vertical Slice (tổ chức theo tính năng, bỏ 4 project) | Dễ làm song song theo module | Trái CONS-001 (bắt buộc 4 tầng) → **loại** |

**Khuyến nghị: A.** Domain **không có NuGet nào** (validator để ở Application — sửa NFR-MAINT-004 cho khớp CONS-001/CONS-008). `IUnitOfWork` và các interface repository khai báo ở một nơi duy nhất (Application). Pipeline MediatR thống nhất: `LoggingBehavior` (gộp luôn phần đo thời gian của PerformanceBehavior) → `ValidationBehavior` → Handler; bỏ Caching/CacheInvalidation behavior (S-07). Câu truy vấn SQL chậm đo bằng EF Core interceptor/trace, không đo trong MediatR behavior.

**Giấy phép MediatR:**

| Phương án | Ưu điểm | Nhược điểm |
|---|---|---|
| **1.** Ghim MediatR 12.x (MIT) | Không phải quản lý license key | Không còn nhận cập nhật |
| **2.** MediatR ≥ 13 + Community License miễn phí (mục đích giáo dục đủ điều kiện) | Được cập nhật; hợp lệ | Phải đăng ký và cấu hình key (không có key thì chỉ ghi log cảnh báo) |
| **3.** Thay bằng thư viện MIT khác hoặc tự viết dispatcher | Không lo giấy phép | Trái CONS-002 (bắt buộc MediatR) |

**Khuyến nghị: 2** (hoặc 1 nếu nhóm không muốn quản lý key) — cả hai đều hợp lệ; ghi rõ phiên bản vào SRS. Không dùng AutoMapper ≥ 15 (giấy phép tương tự) — map thủ công hoặc dùng Mapperly.

### S-14 · Sitemap và background job (D-16, D-17, G-05)

**Sitemap:**

| Phương án | Ưu điểm | Nhược điểm |
|---|---|---|
| **A.** Giữ FR-JOB-003: Hangfire sinh `sitemap.xml` → lưu storage/DB → Nginx proxy tại `/sitemap.xml`; bỏ ping | Giữ nguyên phân công và yêu cầu recurring job | Backend phải biết cấu trúc URL của frontend (cấu hình `SiteBaseUrl`); cần rule Nginx; dữ liệu trễ tới 24 giờ |
| **B.** Next.js tự sinh sitemap (`app/sitemap.ts`) gọi API, có revalidate | Đúng origin; frontend nắm route; không cần file, lock hay ping | Mất FR-JOB-003 → mất một ví dụ recurring job (cần GV đồng ý) |
| **C.** Sitemap theo B; recurring job của Hangfire đổi thành việc có ích thật: dọn refresh token hết hạn, dọn file mồ côi trên storage | Việc nào đúng chỗ nấy; vẫn học được recurring job; giải luôn G-05 | Phải đổi nội dung FR-JOB-003 → cần GV xác nhận |

**Khuyến nghị: C nếu giảng viên đồng ý, không thì A** (không ping, lưu ở một URL cố định, khai báo trong `robots.txt`). Bỏ RedLock khỏi NFR-SCALE-001 (nếu cần chặn chạy trùng thì dùng `[DisableConcurrentExecution]` của Hangfire).

**Bảo vệ Hangfire Dashboard:**

| Phương án | Ưu điểm | Nhược điểm |
|---|---|---|
| **A.** Chỉ bật ở môi trường Development (mặc định Hangfire chỉ cho request từ localhost) | Không phải code thêm | Chạy trong Docker sau Nginx thì request không còn là "local" → sẽ bị từ chối; production không xem được |
| **B.** Cookie riêng cho dashboard: Admin đăng nhập → set cookie → `IDashboardAuthorizationFilter` kiểm tra role Admin | Đúng nghĩa "chỉ Admin" của SRS | Thêm một cơ chế xác thực cần viết |
| **C.** Basic Auth ở Nginx cho đường dẫn `/hangfire` | Nhanh, không đụng code | Không gắn với tài khoản Admin trong hệ thống |

**Khuyến nghị: A ở dev, C cho bản demo, B nếu còn thời gian.**

**Retry và độ tin cậy:** cấu hình `AutomaticRetry` toàn cục `Attempts = 3`, `DelaysInSeconds = [60, 300, 1800]` (khớp FR-JOB-001); FR-JOB-003 cũng dùng 3 lần cho đồng nhất. Sửa 2.6.2: job **không bị mất** vì đã lưu trong PostgreSQL. Mọi job phải idempotent (chạy lại nhiều lần không sao), vì enqueue sau khi commit có thể trùng hoặc thiếu — job dọn file mồ côi định kỳ sẽ xử lý phần còn sót.

### S-15 · Hạ tầng: health check, triển khai, Nginx, rate limit, Docker (D-11, D-12, D-13, D-18, D-19, G-07)

**Readiness:**

| Phương án | Ưu điểm | Nhược điểm |
|---|---|---|
| **A.** `/health/ready` chỉ kiểm tra PostgreSQL; Redis/storage báo `Degraded` ở `/health` | Khớp 2.6.2 và NFR-REL-002 ("Redis down vẫn chạy") | Dashboard giám sát phải đọc được trạng thái Degraded |
| **B.** Giữ Redis trong readiness | Đúng như FR-OBS-001 | Redis lỗi là cả hệ thống ngừng nhận traffic — trái 2.6.2 |

**Khuyến nghị: A.**

**Mục tiêu triển khai:**

| Phương án | Ưu điểm | Nhược điểm |
|---|---|---|
| **A.** Docker Compose trên một máy (máy demo hoặc VPS) | Khớp 1.2.1, 2.6.1, 6.5; vừa sức | Không có scale ngang hay tự phục hồi thật sự |
| **B.** Kubernetes (k3s/minikube) | Khớp các câu NFR nhắc Kubernetes | Quá sức và ngoài trọng tâm môn học |

**Khuyến nghị: A**; chuyển các câu về Kubernetes, MinIO distributed, CDN, scale ngang sang mục "Định hướng mở rộng" (S-16).

**Domain và Nginx:**

| Phương án | Ưu điểm | Nhược điểm |
|---|---|---|
| **A.** Một domain: `/` → frontend:3000, `/api/` → api:8080 | Production **không cần CORS**; cookie cùng origin; một chứng chỉ TLS | Frontend và backend chung một domain |
| **B.** Subdomain `api.<domain>` (5.2) | Tách biệt rõ ràng | Phải cấu hình CORS, credentials, cookie domain; thêm chứng chỉ |

**Khuyến nghị: A.** Ở dev, dùng `rewrites` của Next.js proxy `/api` sang `localhost:5000` để cũng không vướng CORS; nếu vẫn gọi thẳng cổng 5000 thì CORS dev phải có `If-Match` (không cần nếu chọn T2 ở S-04) và `Expose-Headers: Location, X-Correlation-ID, Retry-After`. `/hangfire` được bảo vệ (S-14); `/health` chỉ mở trong mạng nội bộ.

**Rate limiting** (không cần phân tích phương án — sửa trực tiếp):
- `UseForwardedHeaders` với `KnownNetworks` là mạng Docker nội bộ, để lấy đúng IP client thay vì IP của Nginx.
- Phân vùng theo **userId** khi đã đăng nhập, theo **IP** khi chưa đăng nhập. Request server-to-server từ Next.js phải chuyển tiếp `X-Forwarded-For` của người dùng thật; API chỉ tin header này khi request đến từ container frontend.
- Hạn mức theo endpoint thay vì gộp `/auth/*`: login/register 5 lần/phút theo IP+email; refresh 30 lần/phút; `/auth/me` dùng hạn mức chung. Bộ đếm để trong bộ nhớ (một instance). Header `X-RateLimit-*` là tùy chọn; `Retry-After` là bắt buộc.

**Docker Compose** (sửa trực tiếp): ghim phiên bản mọi image; production chỉ `expose` (không publish) PostgreSQL/Redis/storage; Redis đặt `requirepass`; Seq thêm `ACCEPT_EULA=Y` và `SEQ_FIRSTRUN_ADMINPASSWORD`; thay MailHog bằng **Mailpit** (UI 8025, SMTP 1025); Nginx gọi `api:8080`; frontend có hai biến `API_INTERNAL_URL=http://api:8080` (gọi từ server) và `NEXT_PUBLIC_API_BASE_URL` (gọi từ trình duyệt); không prerender dữ liệu từ API lúc `next build` (render theo yêu cầu rồi mới cache); image frontend chạy Node LTS (sửa "Node build only" ở 2.4.1).

### S-16 · Viết lại NFR theo mức đồ án (E-01 → E-06, D-12)

| Phương án | Ưu điểm | Nhược điểm |
|---|---|---|
| **A.** Giữ nguyên NFR | Không phải xin sửa | Nhiều chỉ tiêu không đo được, có chỉ tiêu tính sai; cuối kỳ không chứng minh được |
| **B.** Chia hai tầng: **Bắt buộc** (đo được trên máy demo, có công cụ và kịch bản đo) và **Định hướng mở rộng** (không cam kết) | Trung thực, kiểm chứng được; vẫn giữ tầm nhìn kiến trúc | Cần GV đồng ý với tầng "bắt buộc" |
| **C.** Bỏ các NFR khó | Nhẹ việc | Mất phần "nâng cao" của môn học |

**Khuyến nghị: B.** Gợi ý (nhóm tự điều chỉnh con số):

| NFR | Bắt buộc (đo trên Docker Compose local, dữ liệu seed) | Định hướng mở rộng |
|---|---|---|
| PERF-001/002 | GET công khai p95 ≤ 500 ms với k6 50 VU trong 5 phút; ghi lại cấu hình máy | p99, scale ngang tuyến tính |
| PERF-003 | Có cache + xóa theo tag; báo cáo hit rate từ log/metrics | Hit rate ≥ 80% ở trạng thái ổn định |
| PERF-005 | Lighthouse (mobile) trang chi tiết: LCP ≤ 2,5 s, CLS ≤ 0,1 | CI Lighthouse tự động |
| REL-001 | Sửa con số: 99,5% ≈ **43,8 giờ/năm**; hoặc bỏ SLA vì không có production | Uptime monitor |
| SEC-* | Giữ nguyên (đã chỉnh theo S-05/S-06/S-15) | Quét secret tự động trước commit |
| MAINT-001/002 | `dotnet format` + analyzer mặc định; ESLint `eslint-config-next` + Prettier; unit test cho validator/domain; mỗi endpoint có 1 test thành công + 1 test lỗi; E2E 5 luồng chính | StyleCop/Sonar đầy đủ, coverage ≥ 80% |
| SCALE-* | Một instance; connection pool khớp `max_connections` | Read replica, partitioning, MinIO distributed, CDN |
| SEO-001 | Pass Rich Results Test cho recipe có ảnh; bỏ "star rating" | — |
| USE-001/002 | Breakpoint theo Tailwind mặc định (`md` 768, `lg` 1024, `xl` 1280); WCAG 2.2 AA cho các trang chính | Test BrowserStack |

### S-17 · Phạm vi: bổ sung gì, cắt gì (G-01, G-02, G-03, G-06, B-10, B-11, D-26)

| Phương án | Ưu điểm | Nhược điểm |
|---|---|---|
| **A.** Bổ sung đầy đủ mọi thứ còn thiếu (đặt lại mật khẩu, xác nhận email, quản trị user, trang tác giả, recipe nổi bật…) | Sản phẩm hoàn chỉnh | Vượt sức, chậm phần lõi |
| **B.** Chỉ bổ sung những gì cần để các phần đã có không bị "mồ côi" | Nhất quán với chi phí thấp | Vẫn phải xin xác nhận vài mục |
| **C.** Cắt mọi trường/khái niệm không có FR đi kèm | Tài liệu gọn, sạch | Mất một số tính năng "nhỏ mà có ích" |

**Khuyến nghị: B + C:**

| Hạng mục | Quyết định đề xuất | MoSCoW |
|---|---|---|
| Unarchive, `GET /recipes/{id}`, `GET /me/recipes` | Thêm (S-11) | Must |
| Quên/đặt lại mật khẩu (token của Identity + email qua Hangfire/MailKit) | Thêm nếu kịp — tái sử dụng hạ tầng email của FR-JOB-001 | Should |
| Xác nhận email + policy `VerifiedAuthor` | Bỏ policy; ghi "ngoài phạm vi v1" | Won't |
| Quản trị user | Giữ cột `IsActive`, chỉ đổi qua seed/SQL; login và refresh kiểm tra `IsActive` | Could |
| Đổi email/username có OTP | Ghi "ngoài phạm vi v1" | Won't |
| `Bio` + trang tác giả | Could; nếu không làm thì bỏ `Bio` khỏi 8.1 | Could |
| Recipe "nổi bật" ở trang chủ | Định nghĩa = mới publish gần nhất và có ảnh (không cần thêm trường) | Must |
| Ảnh danh mục, `OrderIndex` danh mục | Could; FR-CAT-001 sắp theo `OrderIndex` rồi `Name` nếu giữ | Could |
| Ảnh cho từng bước | Bỏ `RecipeStep.ImageUrl` ở v1 | Won't |
| AvatarUrl | v1 chỉ lấy từ Google hoặc để trống; không cho nhập URL tùy ý | — |
| Định dạng nội dung | **Plain text** cho mọi trường văn bản (render giữ xuống dòng); bỏ trường "legacy" `Instructions` | Must |
| Sắp xếp lại bước/nguyên liệu | `PUT /recipes/{id}/steps/order` nhận danh sách id | Should |

*Về định dạng nội dung:* plain text là đơn giản và an toàn nhất (React tự escape khi hiển thị); Markdown hiển thị đẹp hơn cho các bước nhưng phải cấu hình renderer không cho HTML thô; HTML/rich text có nguy cơ XSS cao nhất và cần sanitizer.

### S-18 · Phiên bản công nghệ, công cụ và danh sách trình duyệt (F-02, F-03, F-04, F-06, E-03)

| Hạng mục | Đề xuất ghi vào SRS v1.1 | Ghi chú |
|---|---|---|
| .NET | .NET 10 SDK (LTS) | Giữ nguyên |
| IDE | Visual Studio 2026 / VS Code + C# Dev Kit / Rider bản có hỗ trợ .NET 10 | Bỏ "VS 2022 v17.12+" và "Rider 2024+" |
| Node.js | 22 LTS hoặc 24 LTS (dev và runtime production) | Node 20 đã hết hỗ trợ |
| Next.js | Ghim một phiên bản 16.x cụ thể | Bỏ "14+" và "15" |
| Auth.js | Ghim chính xác bản `next-auth@5` beta đã test; theo dõi thông báo bảo mật | Dự phòng theo S-05 |
| Tailwind CSS | 4.x | Quyết định danh sách trình duyệt tối thiểu |
| Trình duyệt | **Một bảng duy nhất:** Chrome/Edge 111+, Firefox 128+, Safari/iOS 16.4+ | Thay cả 2.4.3 lẫn 5.4 |
| Object storage | S3-compatible (S-09) | — |
| Email dev | Mailpit | Thay MailHog |
| Lint | `eslint-config-next` + Prettier | Thay Airbnb ruleset |
| CI | GitHub Actions tối thiểu: build + test + lint cho mỗi PR | SRS nhắc CI nhưng chưa định nghĩa |
| Chuẩn trích dẫn | RFC 9457 (thay 7807), RFC 9110 (thay 7231); ISO/IEC/IEEE 29148 là chuẩn chính | — |

---

## Phần 4. Bảng sửa nhanh cho các lỗi còn lại

| Mã | Cách sửa |
|---|---|
| A-01 | Sửa "27" → "34" ở 1.5, 2.2, mở đầu Chương 3 |
| A-02 | Bổ sung MoSCoW, ngoại lệ, tiêu chí chấp nhận cho 11 FR (có thể dạng bảng rút gọn) |
| A-03 | NFR-SEC = 7; bỏ "FURPS+" hoặc ghi "tham khảo ISO/IEC 25010"; bỏ câu "mỗi NFR có mức ưu tiên" hoặc bổ sung ưu tiên |
| A-04 | Viết lại 1.5 theo nội dung thực tế sau khi sửa |
| A-05 | Thống nhất "Chương 1 → 8"; footer một kiểu; bỏ "CONFIDENTIAL" |
| A-06 | Mọi chữ "hoặc / tùy chọn / có thể" trong yêu cầu phải được chốt hoặc chuyển sang mục "Định hướng" |
| A-07 | Bản Markdown đã đánh dấu `[…]`; điền lại theo quyết định ở S-10 và S-11 (hoặc xuất lại PDF từ file Word nếu còn) |
| A-08 | Gộp hai bảng thuật ngữ; xóa DXA; bỏ CQRS trùng |
| B-06 | Theo S-11 (mục Danh mục) |
| B-07 | Theo S-03 và S-11 |
| B-08 | Chốt theo S-10(e) và S-17: `CookTimeMinutes ≥ 0`; `Difficulty` gồm Easy/Medium/Hard (bỏ Expert, hoặc thêm Expert vào bộ lọc); Quantity/Unit cho phép null ("vừa đủ"), khi có thì Quantity > 0; Name nguyên liệu 1–200; Step có `Title` tùy chọn (≤ 200); bỏ `Instructions`; Description 1–2000; Nutrition đủ 6 trường; DisplayName 2–100; mật khẩu theo NFR-SEC-001 (có chữ thường) và cấu hình đúng vào `IdentityOptions.Password` |
| B-09 | Theo bảng đặt tên ở S-10(e); SDK storage thống nhất `AWSSDK.S3` |
| B-10, B-11 | Theo S-17; Admin được seed với **cả** role Admin và Author, hoặc policy Author chấp nhận cả hai role |
| C-09 | `StepNumber` do server sinh (theo FR-RCP-010); `POST /steps` nhận `{ title?, description, durationMinutes? }`; `PUT /steps/{stepId}` không cho sửa `stepNumber` — đổi thứ tự qua `PUT /recipes/{id}/steps/order` (S-17); `RecipeStep.Create` nhận đủ tham số |
| C-10 | Bổ sung endpoint theo S-11/S-17; bảng 8.3 sinh từ OpenAPI (có Response) |
| C-11 | Ghi rõ "API nghiệp vụ dùng tiền tố /api/v1; `/health`, `/hangfire`, `/scalar` là endpoint vận hành"; thêm `search` vào danh sách slug cấm |
| D-19 | Theo S-15 (image frontend chạy Node LTS) |
| D-20 | Bỏ `IncludeOwned`; dùng projection hoặc `AsSplitQuery()` cho trang chi tiết; sửa câu read replica; pool mỗi instance (API + Hangfire) nhỏ hơn `max_connections` |
| D-21 | Không soft delete (S-03). Đánh số lại bằng hai bước trong một transaction (tạm cộng thêm 1000 vào mọi `StepNumber` rồi gán số mới — không dùng số âm vì 7.3 có `CHECK > 0`), **hoặc** khai báo unique constraint `DEFERRABLE INITIALLY DEFERRED` qua SQL migration; thêm step trong transaction có khóa dòng Recipe (`SELECT … FOR UPDATE`) để tránh trùng `Max + 1` |
| D-22 | Theo S-12 |
| D-24 | Theo S-08 (index) |
| D-25 | Bọc tạo user + role + refresh token trong một transaction (`IDbContextTransaction`); enqueue job sau khi commit; map `DuplicateUserName`/`DuplicateEmail` → 409 |
| D-26 | Theo S-17 (AvatarUrl) |
| D-27 | Thống nhất: user không còn tồn tại hoặc bị vô hiệu hóa → 401 cho mọi endpoint cần xác thực |
| D-28 | Theo S-04 (cập nhật `UpdatedAt` của Recipe khi sửa bảng con) |
| D-29 | Theo S-11 (kiểm tra quyền cho mọi thao tác ghi; Draft của người khác → 404) |
| D-30 | Giữ HS256 cho đơn giản; khóa ≥ 256 bit lấy từ secret; bỏ "xoay khóa 90 ngày" hoặc thêm `kid` + giai đoạn chạy song song hai khóa; frontend không tự xác minh JWT (backend là nơi duy nhất); dùng `JsonWebTokenHandler` |
| D-31 | Log production: Console JSON (Docker thu log) + Seq (một nơi duy nhất); bỏ File sink trong container; một ngưỡng "request chậm" 500 ms; SQL chậm theo dõi qua trace EF Core; sửa "MDC" → "LogContext" |
| D-32 | Chuyển Hangfire khỏi bảng hệ thống ngoài; bỏ "Session Store"; vẽ lại sơ đồ bối cảnh có Nginx, SMTP, Seq |
| E-01 | Sửa ≈ 43,8 giờ/năm (hoặc bỏ SLA — S-16) |
| E-02 → E-06 | Theo S-16; thống nhất dung lượng production một con số; bỏ "1 Gbps, IP tĩnh" |
| F-06 | Theo S-18 |
| G-04 | Thêm ma trận truy vết FR ↔ endpoint ↔ route ↔ test (có thể sinh từ tag trong OpenAPI); sơ đồ trạng thái Recipe (bảng S-11); trang 404/500 |
| G-05 | Theo S-06 và S-14; thêm backup volume storage; ghi rõ múi giờ (UTC); tài khoản Admin seed lấy mật khẩu từ biến môi trường |
| G-06 | Theo S-17 (plain text) |
| G-07 | Theo S-15 |
| G-08 | Theo S-05: middleware bảo vệ `/dashboard`, `/profile`; refresh trong callback `jwt`; lỗi `RefreshAccessTokenError` → buộc đăng nhập lại |

---

## Phần 5. Hướng đi đề xuất

### 5.1 Bốn nguyên tắc

1. **Làm rõ trước, code sau — nhưng không sa lầy.** Chỉ các lỗi *Nghiêm trọng* và các quyết định S-01 → S-10 cần chốt trước khi viết code nghiệp vụ; phần còn lại sửa dần qua Pull Request.
2. **Hợp đồng là một nguồn duy nhất.** Tài liệu Markdown trong repo + OpenAPI sinh từ code. Không ai "sửa tay" Chương 8 nữa.
3. **Đúng và đơn giản trước, nâng cao sau.** Chức năng đúng → bảo mật đúng → cache/SEO/observability → tối ưu. Các NFR khó chuyển sang "Định hướng" (S-16).
4. **Tôn trọng tính chất giáo trình của SRS.** Nhóm tự quyết những gì là *sửa lỗi kỹ thuật* hoặc *làm rõ*; những gì *đổi phạm vi* thì gom lại hỏi giảng viên một lần (5.6).

### 5.2 Tổng hợp quyết định khuyến nghị

| # | Chủ đề | Khuyến nghị | Tự quyết / hỏi GV |
|---|---|---|---|
| S-01 | Quản lý SRS | SRS.md trong repo + Change Request + ADR; OpenAPI thay Chương 8 | Tự quyết |
| S-02 | Chương nào thắng | Bảo mật/kỹ thuật > FR (hành vi) > Ch.7 (kiểu dữ liệu) > Ch.8 (viết lại) | Tự quyết |
| S-03 | Xóa dữ liệu | Hard delete + Archive; bỏ `IsDeleted` | **Hỏi GV** |
| S-04 | Concurrency | `xmin` + `version` trong body → 409 | Tự quyết |
| S-05 | Xác thực FE↔BE | BFF với Auth.js v5; Google chỉ nhận `idToken`; dự phòng: cookie HttpOnly | Tự quyết (báo GV) |
| S-06 | Refresh token | Xoay vòng + FamilyId + ân hạn 30 giây + job dọn | Tự quyết |
| S-07 | Cache | Output Cache + Redis store, xóa theo tag, một bảng TTL; bật ở giai đoạn 3 | Tự quyết |
| S-08 | Tìm kiếm | Text search config tự tạo + unaccent + trigger + GIN; fallback trigram | Tự quyết |
| S-09 | Object storage | Code trung lập S3; dev ghim MinIO/local; demo dùng cloud S3 | **Báo GV** |
| S-10 | API chung | Không vỏ bọc; 422 validation; RFC 9457 + `code`; `sort=-field`; bảng tên | Tự quyết |
| S-11 | Vòng đời recipe | Máy trạng thái + unarchive; publish ≥ 1 step + ≥ 1 ingredient (+ ảnh sau); endpoint công khai chỉ Published; slug tự thêm hậu tố, bất biến sau publish | **Hỏi GV** (điều kiện publish, endpoint mới) |
| S-12 | Ảnh | Upload qua API; kiểm tra chữ ký file; key theo thư mục; thêm og 1200×630 | Tự quyết (AVIF: hỏi GV) |
| S-13 | Kiến trúc | Clean Architecture chặt, Domain 0 NuGet; MediatR có license hợp lệ | Tự quyết |
| S-14 | Sitemap & job | Sitemap bằng Next.js; recurring job dọn dẹp | **Hỏi GV** |
| S-15 | Hạ tầng | Compose 1 máy; một domain; readiness chỉ DB; rate limit sau proxy | Tự quyết |
| S-16 | NFR | Hai tầng: Bắt buộc / Định hướng | **Hỏi GV** |
| S-17 | Phạm vi | Bổ sung tối thiểu + cắt phần mồ côi | **Hỏi GV** |
| S-18 | Phiên bản | Bảng phiên bản và danh sách trình duyệt thống nhất | Tự quyết |

### 5.3 Lộ trình theo giai đoạn

**Giai đoạn 0 — Chốt quyết định (2–3 ngày, cả nhóm)**
- Một buổi họp đọc 9 lỗi Nghiêm trọng và bảng 5.2; chốt S-01 → S-10 (mỗi quyết định viết một ADR khoảng nửa trang).
- Gửi giảng viên danh sách câu hỏi ở 5.6.
- Tạo `docs/SRS_v1.1.md` từ bản Markdown, ghi bảng Change Request.
- Viết khung hợp đồng chung: bảng mã lỗi + Problem Details, `PagedResult`, quy ước đặt tên, danh sách endpoint (stub).
- **Kết quả:** không còn "hoặc" trong các quyết định chặn; mọi người code theo cùng một hợp đồng.

**Giai đoạn 1 — Nền tảng dùng chung (khoảng 1 tuần)**
- Backend: solution 4 project + architecture test; `BaseEntity` (không `IsDeleted`, có `xmin`); `AuditInterceptor`; `IExceptionHandler` + Problem Details; `ForwardedHeaders`; `ICurrentUser`; migration đầu tiên.
- Hạ tầng: `docker-compose.yml` đã sửa (ghim phiên bản, Mailpit, Seq có EULA, storage S3, Redis có mật khẩu); seed Admin (mật khẩu lấy từ biến môi trường) và vài Author.
- Frontend: Next.js 16 + Tailwind 4; API client có hai base URL; TanStack Query; layout; `rewrites` proxy `/api`.
- **TV1 giao sớm nhất:** cấu hình xác thực JWT dùng chung + endpoint login → TV2/TV3 test được endpoint cần đăng nhập ngay từ tuần đầu.
- **TV2 giao sớm:** entity `Recipe` + migration → TV3 (ảnh) và TV4 (search) có bảng để làm việc.

**Giai đoạn 2 — Làm các module song song**
- TV1: register/login/refresh/logout → Google → me/profile → welcome email.
- TV2: CRUD recipe + máy trạng thái + steps/ingredients + concurrency + `me/recipes`.
- TV3: categories → upload ảnh + chữ ký file + storage key → job resize.
- TV4: danh sách công khai + lọc/sắp xếp/phân trang → full-text search → health/logging.
- Mọi PR đi kèm: test cho handler/validator + cập nhật OpenAPI (tự sinh) + ghi mã lỗi mới vào bảng.

**Giai đoạn 3 — Tích hợp và phần "nâng cao"**
- Output Cache + xóa theo tag (S-07); ISR cho trang công khai; JSON-LD/Open Graph; sitemap (S-14); rate limit theo S-15; OpenTelemetry → Seq; Hangfire Dashboard có bảo vệ.

**Giai đoạn 4 — Kiểm thử, đo và chốt tài liệu**
- Integration test mỗi endpoint (1 thành công + 1 lỗi); E2E 5 luồng chính; k6 và Lighthouse theo NFR tầng "Bắt buộc"; cập nhật `SRS_v1.1.md` lần cuối, đánh dấu CR đã đóng.

### 5.4 Phụ thuộc giữa các thành viên

| Ai cần | Cần gì | Từ ai | Khi nào |
|---|---|---|---|
| TV2, TV3, TV4 | Xác thực JWT dùng chung, user seed, login | TV1 | Cuối giai đoạn 1 |
| TV3, TV4 | Entity/migration `Recipe`, `Category` | TV2, TV3 | Giai đoạn 1 |
| TV1 (FR-JOB-001), TV3 (FR-JOB-002) | Cấu hình Hangfire + PostgreSQL storage dùng chung | Chung | Giai đoạn 1 |
| TV2 (điều kiện có ảnh) | API ảnh | TV3 | Giai đoạn 2 → bật `RequireImage` ở giai đoạn 3 |
| TV4 (sitemap, JSON-LD) | Route và DTO chi tiết recipe ổn định | TV2 | Giai đoạn 2 |
| Cả nhóm | Bảng mã lỗi, `PagedResult`, quy ước tên | Chung | Giai đoạn 0 |

### 5.5 Riêng phần việc của TV1 (Auth & Profile) — các bước nhỏ theo thứ tự

TV1 dính nhiều lỗi nhất (23 mã, trong đó 3 lỗi Nghiêm trọng: C-01, C-03, D-05), nên nên đi từng bước:

1. Cùng nhóm chốt **S-05** và **S-06** (ghi ADR).
2. **Spike 1–2 ngày:** Next.js + Auth.js v5 Credentials gọi một endpoint login giả trên .NET; thử refresh trong callback `jwt`. Chạy ổn thì giữ phương án C; vướng thì chuyển sang B.
3. ASP.NET Identity (`AddIdentityCore` + `AddSignInManager`) + `JwtService` → `POST /auth/register` (có transaction) và `POST /auth/login` (lockout đúng cách, kiểm tra `IsActive`).
4. Bảng `RefreshTokens` theo S-06 → `POST /auth/refresh` (xoay vòng + ân hạn) → `POST /auth/logout`.
5. `POST /auth/google { idToken }` (xác minh ID token + `email_verified`) → Google provider phía Auth.js.
6. `GET/PATCH /auth/me` với `displayName`.
7. `WelcomeEmailJob` (Hangfire + MailKit → Mailpit) cho cả đăng ký thường lẫn đăng ký qua Google.
8. Middleware bảo vệ `/dashboard`, `/profile`; xử lý khi refresh thất bại.
9. *(Nếu kịp)* quên/đặt lại mật khẩu.

### 5.6 Câu hỏi nên gửi giảng viên (gom một lần)

1. SRS có cho phép nhóm phát hành **bản làm rõ v1.1** (kèm danh sách thay đổi) không, hay phải bám nguyên văn v1.0? - Cho phép
2. **Xóa công thức:** chấp nhận hard delete + Archive (FR-RCP-007), hay bắt buộc soft delete (Chương 7/8)? 
3. **Điều kiện publish:** ≥ 1 bước (FR), hay ≥ 1 bước + ≥ 1 nguyên liệu (Phụ lục B), có bắt buộc ảnh không? - Không bắt buộc cho phép bổ xung sau
4. **MinIO** đã ngừng phát hành bản cộng đồng: được dùng dịch vụ tương thích S3 khác (hoặc lưu local ở môi trường dev) không? - Nghiên cứu cloudflare Tunnel
5. **FR-JOB-003:** được chuyển việc sinh sitemap sang Next.js và thay recurring job bằng job dọn dẹp không? - Chọn phương án phù hợp
6. **NFR:** đồng ý chia "Bắt buộc (đo được trên máy demo)" và "Định hướng mở rộng" không? - Làm base trước, hoàn thiện làm mở rộng sau
7. **Phạm vi:** đồng ý thêm unarchive, `GET /recipes/{id}`, `GET /me/recipes`; loại bỏ xác nhận email/OTP/ảnh cho từng bước khỏi v1 không? - Chọn phương pháp tối ưu
8. **Định dạng ảnh:** có bắt buộc hỗ trợ AVIF không? - Không

### 5.7 Rủi ro còn lại và dấu hiệu cần đổi hướng

| Rủi ro | Dấu hiệu | Hành động |
|---|---|---|
| Auth.js v5 (beta) gây lỗi khó gỡ | Spike quá 2 ngày vẫn chưa refresh ổn định | Chuyển S-05 sang phương án B |
| Image MinIO không kéo được nữa | `docker compose pull` lỗi | Dùng storage local (dev) hoặc dịch vụ S3 khác — code không phải đổi nhờ S-09 |
| Full-text search tiếng Việt cho kết quả kém | Tìm "pho" ra quá nhiều "phố" | Ưu tiên Title (trọng số A), thêm fallback trigram (S-08 phương án C) |
| Trễ tiến độ | Hết giai đoạn 2 mà luồng chính chưa chạy | Hoãn phần Could/Should của S-17 và các mục "Định hướng" của S-16; giữ phần Must |
| Hợp đồng API lại lệch nhau | Frontend gặp lỗi kiểu dữ liệu khi ghép | Bắt buộc sinh kiểu TypeScript từ OpenAPI trong CI |

---

## Phụ lục — Đối chiếu mã lỗi ↔ giải pháp

Tất cả **82** mã lỗi đều có hướng xử lý.

| Mã lỗi | Mức độ | Xử lý tại |
|---|---|---|
| A-01 | Cao | S-01, Phần 4 |
| A-02 | Trung bình | S-01, Phần 4 |
| A-03 | Thấp | S-01, Phần 4 |
| A-04 | Thấp | S-01, Phần 4 |
| A-05 | Thấp | S-01, Phần 4 |
| A-06 | Trung bình | S-01, Phần 4 |
| A-07 | Trung bình | S-01, Phần 4 |
| A-08 | Thấp | S-01, Phần 4 |
| B-01 | Nghiêm trọng | S-03 |
| B-02 | Cao | S-11 |
| B-03 | Cao | S-11 |
| B-04 | Cao | S-11 |
| B-05 | Cao | S-11 |
| B-06 | Cao | Phần 4 |
| B-07 | Trung bình | S-03, S-11, Phần 4 |
| B-08 | Cao | S-10, Phần 4 |
| B-09 | Cao | S-10, Phần 4 |
| B-10 | Trung bình | S-17, Phần 4 |
| B-11 | Trung bình | S-17, Phần 4 |
| C-01 | Nghiêm trọng | S-05 |
| C-02 | Cao | S-05 |
| C-03 | Nghiêm trọng | S-05 |
| C-04 | Cao | S-10 |
| C-05 | Cao | S-10 |
| C-06 | Cao | S-10 |
| C-07 | Cao | S-04 |
| C-08 | Cao | S-12 |
| C-09 | Cao | Phần 4 |
| C-10 | Trung bình | S-10, Phần 4 |
| C-11 | Thấp | Phần 4 |
| D-01 | Nghiêm trọng | S-04 |
| D-02 | Nghiêm trọng | S-08 |
| D-03 | Nghiêm trọng | S-07 |
| D-04 | Nghiêm trọng | S-07 |
| D-05 | Nghiêm trọng | S-05 |
| D-06 | Cao | S-05 |
| D-07 | Cao | S-06 |
| D-08 | Cao | S-05 |
| D-09 | Cao | S-05 |
| D-10 | Cao | S-13 |
| D-11 | Cao | S-15 |
| D-12 | Cao | S-15, S-16 |
| D-13 | Cao | S-15 |
| D-14 | Cao | S-12 |
| D-15 | Cao | S-12 |
| D-16 | Cao | S-14 |
| D-17 | Cao | S-14 |
| D-18 | Cao | S-15 |
| D-19 | Cao | S-15, Phần 4 |
| D-20 | Trung bình | Phần 4 |
| D-21 | Trung bình | S-03, Phần 4 |
| D-22 | Trung bình | S-12, Phần 4 |
| D-23 | Trung bình | S-08 |
| D-24 | Trung bình | S-08, Phần 4 |
| D-25 | Trung bình | Phần 4 |
| D-26 | Trung bình | S-17, Phần 4 |
| D-27 | Thấp | Phần 4 |
| D-28 | Trung bình | S-04, Phần 4 |
| D-29 | Thấp | S-11, Phần 4 |
| D-30 | Thấp | Phần 4 |
| D-31 | Trung bình | Phần 4 |
| D-32 | Thấp | Phần 4 |
| E-01 | Cao | S-16, Phần 4 |
| E-02 | Trung bình | S-16, Phần 4 |
| E-03 | Trung bình | S-01, S-16, S-18, Phần 4 |
| E-04 | Trung bình | S-16, Phần 4 |
| E-05 | Thấp | S-16, Phần 4 |
| E-06 | Thấp | S-16, Phần 4 |
| F-01 | Nghiêm trọng | S-09 |
| F-02 | Cao | S-18 |
| F-03 | Cao | S-05, S-18 |
| F-04 | Cao | S-18 |
| F-05 | Trung bình | S-13 |
| F-06 | Thấp | S-18, Phần 4 |
| G-01 | Cao | S-17 |
| G-02 | Trung bình | S-17 |
| G-03 | Trung bình | S-17 |
| G-04 | Trung bình | Phần 4 |
| G-05 | Trung bình | S-14, Phần 4 |
| G-06 | Trung bình | S-17, Phần 4 |
| G-07 | Trung bình | S-15, Phần 4 |
| G-08 | Thấp | S-05, Phần 4 |

## Nguồn tham khảo

- Npgsql — Concurrency Tokens: https://www.npgsql.org/efcore/modeling/concurrency.html
- Next.js — Supported Browsers: https://nextjs.org/docs/architecture/supported-browsers
- Lucky Penny Software — MediatR Licensing FAQ: https://luckypennysoftware.com/faq
- Auth.js gia nhập Better Auth: https://github.com/nextauthjs/next-auth/discussions/13252
- Dòng thời gian MinIO bản cộng đồng: https://blog.vonng.com/en/db/minio-resurrect/
- Google — Sitemaps ping endpoint is going away: https://developers.google.com/search/blog/2023/06/sitemaps-lastmod-ping
- Seq — Docker: https://datalust.co/docs/getting-started-with-docker


