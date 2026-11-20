# BÁO CÁO DỰ ÁN CULINARY BLOG

**Học phần:** Phát triển Ứng dụng Web Nâng cao  
**Nhóm:** 17  
**Ngày lập báo cáo:** 20/11/2026  
**Kho mã nguồn:** [BaoThw05/PTUDWNC-2026-Nhom17](https://github.com/BaoThw05/PTUDWNC-2026-Nhom17)

## 1. Giới thiệu đề tài và phạm vi nghiên cứu

### 1.1. Giới thiệu đề tài

Culinary Blog là ứng dụng web chia sẻ công thức nấu ăn. Người dùng có thể đăng ký, đăng nhập, tạo và quản lý công thức; công chúng có thể duyệt công thức theo danh mục và tìm kiếm bằng tiếng Việt có dấu hoặc không dấu. Hệ thống hỗ trợ ảnh công thức, thông tin nguyên liệu, các bước chế biến, phân trang và các nội dung phục vụ SEO.

### 1.2. Mục tiêu

- Xây dựng sản phẩm full-stack gồm giao diện web, REST API và các dịch vụ dữ liệu.
- Tổ chức backend theo Clean Architecture, tách nghiệp vụ khỏi hạ tầng.
- Cung cấp luồng quản lý công thức từ tạo bản nháp đến xuất bản, lưu trữ và khôi phục.
- Tạo môi trường chạy lặp lại được bằng Docker Compose, phục vụ phát triển và trình diễn.
- Kiểm tra các yêu cầu phi chức năng cơ bản về hiệu năng, SEO và khả năng vận hành.

### 1.3. Phạm vi

Phạm vi chức năng được chia thành các module:

- **Xác thực và hồ sơ:** đăng ký, đăng nhập, phiên/token, hồ sơ người dùng và chức năng quản trị liên quan.
- **Công thức:** tạo, xem, sửa, xuất bản, lưu trữ, xóa mềm/khôi phục; quản lý nguyên liệu và các bước.
- **Danh mục:** duyệt danh mục và quản lý danh mục.
- **Tệp và ảnh:** tải lên, chọn ảnh chính, xóa ảnh và lưu trữ tương thích S3.
- **Danh sách và tìm kiếm:** lọc, sắp xếp, phân trang và tìm kiếm tiếng Việt không dấu.
- **SEO và vận hành:** metadata, dữ liệu có cấu trúc, sitemap, health checks, logging và cache.

Hạng mục mở rộng như tracing/metrics OpenTelemetry, tìm kiếm nâng cao theo nguyên liệu và tối ưu hiệu năng vượt mức cơ bản không được xem là điều kiện đã hoàn thành trong báo cáo này.

## 2. Kiến trúc, thiết kế hệ thống và nền tảng công nghệ

### 2.1. Kiến trúc tổng thể

Ứng dụng dùng kiến trúc web phân lớp. Trình duyệt truy cập Next.js; frontend gọi backend REST API. Khi triển khai bằng Compose, Nginx là cổng vào chung và định tuyến đến frontend, API và tài nguyên ảnh.

```text
Trình duyệt
    │
    ▼
Nginx ─── /             ──► Next.js 16
    ├──── /api, /health ──► ASP.NET Core API
    └──── /media        ──► MinIO (S3-compatible)
                                │
ASP.NET Core API ──► PostgreSQL 16
                 ├─► Redis 7
                 ├─► Hangfire (background jobs)
                 └─► Seq / Mailpit (log và email phát triển)
```

### 2.2. Thiết kế backend

Backend được chia thành bốn project, với chiều phụ thuộc hướng vào nghiệp vụ:

1. **Domain:** entity và quy tắc miền; hạn chế phụ thuộc thư viện bên ngoài.
2. **Application:** use case, CQRS command/query, handler, validation và abstraction.
3. **Infrastructure:** Entity Framework Core, PostgreSQL, Identity, Redis, lưu trữ tệp và các dịch vụ nền.
4. **API:** ASP.NET Core Minimal APIs, ánh xạ endpoint, xác thực, xử lý lỗi và OpenAPI.

MediatR chuyển command/query đến handler; FluentValidation kiểm tra dữ liệu; lỗi API theo Problem Details và mã lỗi nghiệp vụ. PostgreSQL lưu dữ liệu ứng dụng; Redis phục vụ cache. Công thức bị xóa được xử lý theo cơ chế xóa mềm/thùng rác; các thao tác cập nhật có kiểm soát phiên bản để phát hiện xung đột.

### 2.3. Thiết kế frontend

Frontend sử dụng Next.js App Router và TypeScript. Các trang được tổ chức theo khu vực tính năng như xác thực, hồ sơ, dashboard công thức, danh mục, danh sách và tìm kiếm. API client tập trung trong thư viện dùng chung; một số luồng xác thực dùng mô hình BFF/proxy để hạn chế đưa token xuống trình duyệt. Các trang công thức hỗ trợ metadata, canonical URL, Open Graph và JSON-LD.

### 2.4. Nền tảng công nghệ

| Thành phần | Công nghệ |
|---|---|
| Backend | .NET 10, ASP.NET Core Minimal APIs, C# |
| Kiến trúc/use case | Clean Architecture, CQRS, MediatR 12.5.0, FluentValidation |
| ORM/cơ sở dữ liệu | Entity Framework Core, PostgreSQL 16 |
| Frontend | Next.js 16, React 19, TypeScript, Tailwind CSS 4 |
| Xác thực frontend | Auth.js v5 |
| Cache | Redis 7, Output Cache |
| Lưu ảnh | MinIO tương thích S3; Nginx phục vụ đường dẫn `/media` |
| Tác vụ nền | Hangfire với PostgreSQL |
| Reverse proxy/demo | Nginx, tùy chọn Cloudflare Tunnel |
| Logging/email phát triển | Serilog, Seq, Mailpit |
| Kiểm thử | xUnit, integration/architecture tests, Playwright, k6 |
| Đóng gói | Docker Compose |

## 3. Phân công công việc

| Thành viên | Phụ trách chính | Công việc cụ thể |
|---|---|---|
| **TV1 – Nguyễn Ngọc Tuấn** | Xác thực, hồ sơ, người dùng | ASP.NET Core Identity, JWT/refresh token, đăng ký/đăng nhập/đăng xuất, hồ sơ, luồng Auth.js, email chào mừng; tổng hợp SRS v1.1 và ADR. |
| **TV2 – Bùi Ngọc Toàn** | Recipe Core và nền backend | Solution backend bốn tầng, entity nền, xử lý lỗi và hợp đồng API; nghiệp vụ công thức, nguyên liệu, bước, vòng đời xuất bản, concurrency, thùng rác và khôi phục. |
| **TV3 – Lương Đức Sang** | Danh mục, ảnh và hạ tầng | API/UI danh mục; tải lên và quản lý ảnh; object storage; Docker Compose, Hangfire, Nginx, Cloudflare Tunnel; backup/restore. |
| **TV4 – Trần Lê Bảo Thư** | Tìm kiếm, SEO, observability và nền frontend | Next.js, API client, CI; danh sách/tìm kiếm; cache, logging, health checks, SEO, sitemap; E2E và đo NFR. |

Phân công theo thư mục được quy định trong [OWNERSHIP.md](./OWNERSHIP.md); kế hoạch chi tiết từng thành viên nằm trong [docs/KeHoach](./KeHoach/).

## 4. Kết quả thực hiện

Đánh giá dưới đây dựa trên cấu trúc mã nguồn, tài liệu và bằng chứng đã có trong repository tại ngày lập báo cáo. Việc một module có mã nguồn không đồng nghĩa mọi tiêu chí nghiệm thu của module đã được xác nhận.

### 4.1. Hạng mục đã có kết quả/bằng chứng

| Hạng mục | Kết quả |
|---|---|
| Khung ứng dụng | Có backend bốn tầng, frontend Next.js, cấu hình Docker Compose và các dịch vụ dữ liệu/vận hành. |
| Phân hệ chức năng | Repository có các module API Auth, Categories, Observability, RecipeImages, Recipes, RecipeSearch; frontend có các route xác thực, hồ sơ, dashboard, danh mục, công thức và tìm kiếm. |
| Kiểm thử | Có unit, integration và architecture test projects phía backend; có các kịch bản Playwright E2E cho công thức, tìm kiếm và SEO. |
| Đo hiệu năng API | Báo cáo NFR ghi nhận k6 chạy 50 VU trong 5 phút: 44.701 request, 148,63 request/giây, p95 5,30 ms, tỷ lệ lỗi 0%, đạt ngưỡng đã đặt trong kịch bản. |
| Kiểm tra UI/SEO local | Báo cáo NFR ghi nhận các trang kiểm tra trả HTTP 200, canonical/Open Graph/Twitter Card hiện diện, ảnh 16/16 tải được và JSON-LD Recipe có dữ liệu. |
| Tối ưu Lighthouse | Điểm Performance được ghi nhận là 92, SEO 100, Accessibility 100, Best Practices 100; CLS 0. |
| Hướng dẫn vận hành | README có hướng dẫn chạy Compose, backend/frontend riêng, cổng dịch vụ và cấu hình Tunnel; có tài liệu backup/restore. |

Chi tiết số đo và bằng chứng được lưu trong [báo cáo đo NFR](./bao-cao-do-nfr.md) và [docs/evidence/tv4](./evidence/tv4/).

### 4.2. Hạng mục chưa hoàn thành hoặc cần xác nhận thêm

| Hạng mục | Tình trạng |
|---|---|
| LCP Lighthouse mobile | **Chưa đạt ngưỡng:** LCP mô phỏng 3.350 ms, trong khi mục tiêu là không quá 2.500 ms. LCP quan sát tại lần chạy local là 294 ms; hai giá trị được giữ riêng trong báo cáo gốc. |
| Google Rich Results | **Chưa xác minh:** thử nghiệm bằng mã bị yêu cầu đăng nhập; URL local và ảnh localhost không thể được Google truy cập như một trang public. Cần chạy lại với demo công khai và ảnh truy cập công khai. |
| SRS v1.1 | **Còn ở trạng thái nháp:** bảng Change Request vẫn đánh dấu CR-01 đang xử lý và CR-18 chưa hoàn tất. |
| Nghiệm thu toàn bộ kế hoạch | **Chưa đủ căn cứ để kết luận hoàn tất 100%:** checklist trong các kế hoạch cá nhân còn nhiều ô chưa đánh dấu. Cần nhóm rà soát và cập nhật trạng thái theo bằng chứng mới nhất. |
| Build/test toàn dự án tại thời điểm báo cáo | Báo cáo này không chạy lại toàn bộ build, test, lint hoặc E2E; kết quả NFR được trích từ bằng chứng hiện có trong repository. |

Vì vậy, báo cáo ghi nhận ứng dụng và nhiều phân hệ đã được xây dựng, đồng thời không xem các mục kiểm thử/ nghiệm thu còn thiếu bằng chứng là đã hoàn thành.

## 5. Liên kết mã nguồn

- Repository GitHub: [https://github.com/BaoThw05/PTUDWNC-2026-Nhom17](https://github.com/BaoThw05/PTUDWNC-2026-Nhom17)
- Hướng dẫn cài đặt và chạy: [README.md](../README.md)
- Đặc tả yêu cầu: [SRS_v1.1.md](./SRS_v1.1.md)
- Phân vùng trách nhiệm: [OWNERSHIP.md](./OWNERSHIP.md)
- Báo cáo NFR: [bao-cao-do-nfr.md](./bao-cao-do-nfr.md)
- Sao lưu và khôi phục: [backup-restore.md](./backup-restore.md)

## 6. Hướng dẫn cài đặt và chạy chương trình

### 6.1. Yêu cầu môi trường

- Git.
- .NET SDK 10 (phiên bản được ghim trong `global.json`).
- Node.js từ 20.9 trở lên, khuyến nghị Node.js 22; npm.
- Docker Desktop có Docker Compose v2. Trên Windows bật WSL2 và khởi động Docker Desktop trước khi chạy container.

### 6.2. Chạy toàn bộ hệ thống bằng Docker Compose

Từ thư mục gốc repository, tạo file môi trường:

```powershell
Copy-Item .env.example .env
```

Mở `.env`, thay các mật khẩu và `AUTH_SECRET` bằng giá trị riêng của môi trường chạy. Không commit file `.env` hoặc chia sẻ secret. Sau đó chạy:

```powershell
docker compose up -d --build
docker compose ps
```

Truy cập:

- Web: `http://localhost`
- Kiểm tra API: `http://localhost/health`
- Tài liệu API: `http://localhost/scalar`
- Seq: `http://localhost:5341`
- Mailpit: `http://localhost:8025`
- MinIO Console: `http://localhost:9001`

Nếu cổng mặc định đang được sử dụng, chỉnh biến cổng tương ứng trong `.env`. Tham khảo [README mục cài đặt và chạy](../README.md#1-cài-đặt).

### 6.3. Chạy backend và frontend riêng

Khởi động các dịch vụ phụ thuộc bằng Docker Compose trước; sau đó mở terminal backend:

```powershell
Set-Location src\Backend
dotnet build
dotnet test
dotnet run --project CulinaryBlog.API
```

API chạy tại `http://localhost:5000`; health check tại `/health`, tài liệu API tại `/scalar`.

Mở terminal khác cho frontend:

```powershell
Set-Location src\Frontend
Copy-Item .env.example .env.local
npm install
npm run dev
```

Frontend chạy tại `http://localhost:3000`. Cách chạy riêng cần các dịch vụ nền trong Docker và cấu hình môi trường phù hợp; tham khảo [README](../README.md).

## 7. Kết luận

Culinary Blog đã có nền tảng full-stack gồm ứng dụng .NET 10, frontend Next.js 16, PostgreSQL, Redis, lưu trữ ảnh và cấu hình triển khai Docker Compose. Mã nguồn thể hiện các phân hệ cốt lõi cho xác thực, công thức, danh mục, ảnh, tìm kiếm và SEO. Bằng chứng đo hiện có cho thấy API đạt ngưỡng p95 trong kịch bản k6 và các kiểm tra smoke UI/SEO local đạt yêu cầu.

Dự án chưa nên được xem là nghiệm thu hoàn toàn: LCP Lighthouse mô phỏng còn vượt ngưỡng, Rich Results chưa được xác minh trên môi trường public, SRS v1.1 vẫn là bản nháp và checklist của nhóm cần được đối chiếu/cập nhật. Bước tiếp theo là tối ưu LCP, chạy lại Rich Results trên demo công khai, hoàn tất SRS và thực hiện một lượt build/test/E2E nghiệm thu có ghi nhận kết quả.
