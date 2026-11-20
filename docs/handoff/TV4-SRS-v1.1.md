# Bàn giao phần TV4 cho SRS v1.1

Ngày rà soát: 20/11/2026
Tài liệu chuẩn: [`SRS_v1.1.md`](../SRS_v1.1.md)

Phần dưới đây đã được đối chiếu với code hiện tại để TV1 tích hợp vào bản SRS v1.1:

| Phần SRS | Nội dung chốt / đối chiếu | Mã nguồn tham chiếu |
|---|---|---|
| FR-RCP-001 | Danh sách công khai chỉ gồm Published và chưa xóa; filter, sort whitelist, phân trang; Output Cache 5 phút, vary theo query string, bỏ qua request có Authorization. | `GetRecipes.cs`, `Program.cs`, `RecipesEndpoints.cs` |
| 3.4 FR-SRCH | Tìm kiếm không dấu bằng PostgreSQL `unaccent` + `pg_trgm` trên `SearchText`, GIN index; chuẩn hóa từ khóa và chỉ trả nội dung công khai. | `RecipeSearchRepository.cs`, migration `RecipeSearch`, `SearchRecipes.cs` |
| 3.7 FR-OBS | Serilog JSON Console + Seq; Correlation ID, request/status/elapsed time, UserId; không log token/mật khẩu/body nhạy cảm. `/health` tổng hợp DB/Redis/storage, `/health/live` chỉ process, `/health/ready` chỉ PostgreSQL. Redis/storage lỗi báo Degraded. OTel traces/metrics là phần mở rộng. | `Program.cs`, `CorrelationIdMiddleware.cs`, `DependencyHealthChecks.cs`, `LoggingBehavior.cs` |
| Chương 4 NFR | NFR Base đo trên máy demo; API p95 ≤ 500 ms ở 50 VU/5 phút; frontend LCP ≤ 2,5 s, CLS ≤ 0,1, INP ≤ 200 ms; cache policy theo ADR-007. Uptime tham chiếu 99,5% tương đương tối đa khoảng 43,8 giờ/năm, không phải SLA production. | `docs/ADR/ADR-007.md`, `docs/bao-cao-do-nfr.md` |
| 2.4.3 và 5.4 | Duy trì bảng browser support: Chrome/Edge 111+, Firefox 128+, Safari 16.4+; không hỗ trợ IE. Đây là mức tương thích mục tiêu, không phải tuyên bố đã chạy ma trận kiểm thử mọi browser. | `src/Frontend/package.json`, Playwright E2E dùng Chromium |
| 6.1–6.3 | Nginx reverse proxy đến Next.js :3000 và API :8080; PostgreSQL 16, Redis 7, storage S3-compatible. CQRS dùng MediatR; Logging/Validation ở pipeline. ASP.NET Output Cache là middleware ngoài MediatR, dùng Redis/tag invalidation. | `docker-compose.yml`, `Program.cs`, `DependencyInjection.cs`, `RecipesEndpoints.cs` |
| 8.7 | `/health` báo trạng thái tổng hợp, `/health/live` liveness process, `/health/ready` readiness PostgreSQL. Readiness không phụ thuộc Redis hoặc object storage. | `Program.cs`, `DatabaseHealthCheck` |

Trong lần sửa này, các mô tả sai về API port, Nginx, vị trí `ApplicationUser`, MediatR CachingBehavior và phạm vi readiness đã được chỉnh trực tiếp trong `SRS_v1.1.md`. Trạng thái CR-19 tiếp tục là ✅; CR-17 mô tả đúng trigram. Các CR thuộc TV1/TV3 không đổi trạng thái thay TV1.

## Trạng thái chuyển giao

Bản bàn giao đã sẵn sàng cho TV1 nhập vào tài liệu tổng hợp. Không có kênh gửi tài liệu tới thành viên khác được cấu hình trong phiên làm việc này.
