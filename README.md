# Culinary Blog

Blog chia sẻ công thức nấu ăn — đồ án môn Phát triển Ứng dụng Web Nâng cao của Nhóm 17.

Người dùng có thể đăng ký, viết và xuất bản công thức (kèm ảnh, nguyên liệu, các bước làm), duyệt theo danh mục và tìm kiếm tiếng Việt không dấu. Backend viết bằng .NET 10 (Minimal API, Clean Architecture, MediatR), frontend dùng Next.js 16, dữ liệu nằm trong PostgreSQL 16 và Redis 7.

**Mục tiêu của nhóm: đến Chủ nhật 01/11/2026 có một ứng dụng hoàn thiện** — đăng nhập chạy thật, giao diện đầy đủ cho mọi trang, web gần như hoàn chỉnh từ đầu đến cuối.

Tài liệu nằm trong `docs/`:

- [SRS v1.0.1](./docs/SRS_Culinary_Blog_v1.0.1.md) và [các quyết định đã chốt](./docs/SRS_Culinary_Blog_v1.0.0_GiaiPhap.md)
- [Kế hoạch tổng và kế hoạch từng người](./docs/KeHoach/)
- [Ai phụ trách thư mục nào](./docs/OWNERSHIP.md)
- [Bảng mã lỗi API](./docs/api/error-codes.md)

## Mục lục

1. [Cài đặt](#1-cài-đặt)
2. [Chạy dự án](#2-chạy-dự-án)
3. [Cổng dịch vụ](#3-cổng-dịch-vụ)
4. [Cấu trúc thư mục](#4-cấu-trúc-thư-mục)
5. [Thêm tính năng mới](#5-thêm-tính-năng-mới)
6. [Làm việc nhóm](#6-làm-việc-nhóm)
7. [Quy ước viết code](#7-quy-ước-viết-code)
8. [Tiến độ](#8-tiến-độ)
9. [Để nghiên cứu sau](#9-để-nghiên-cứu-sau)
10. [Thành viên](#thành-viên)

## 1. Cài đặt

Bạn cần có:

- .NET SDK 10 (repo đã ghim bằng `global.json`)
- Node.js 20.9 trở lên, khuyến nghị bản 22 (`src/Frontend/.nvmrc`) và npm 10
- Docker Desktop có Compose v2. Trên Windows nhớ bật WSL2 và mở Docker Desktop trước khi chạy compose.
- Git
- `dotnet-ef` nếu cần tạo migration: `dotnet tool install --global dotnet-ef`

## 2. Chạy dự án

Các lệnh dưới đây chạy từ thư mục gốc repo, dùng được cả trên PowerShell lẫn bash.

**Dịch vụ phát triển (chạy trong 5 phút)**

```bash
cp .env.example .env          # PowerShell: Copy-Item .env.example .env
docker compose up -d
docker compose ps             # đợi postgres, redis, minio báo healthy; minio-init Exited (0)
```

Sau khi khởi động, mở ứng dụng tại http://localhost. Nginx chuyển tiếp giao diện
Next.js, API và health check; không cần chạy `dotnet run` hay `npm run dev` trên máy.
PostgreSQL, Redis, MinIO, Seq và Mailpit đã sẵn sàng. MinIO
tự tạo bucket `culinary-blog`; mở MinIO Console tại http://localhost:9001,
Seq tại http://localhost:5341 và Mailpit tại http://localhost:8025.

**Backend**

```bash
cd src/Backend
dotnet build
dotnet test
dotnet run --project CulinaryBlog.API
```

API chạy ở http://localhost:5000. Mở http://localhost:5000/health để kiểm tra (trả về `Healthy`), tài liệu API ở http://localhost:5000/scalar.

Backend đọc connection string trong `appsettings.Development.json`, khớp sẵn với `.env.example`. Nếu bạn đổi mật khẩu trong `.env`, báo cho backend bằng user-secrets (không commit):

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=culinary_blog;Username=culinary_admin;Password=<mật khẩu>" --project CulinaryBlog.API
```

**Frontend**

```bash
cd src/Frontend
cp .env.example .env.local    # PowerShell: Copy-Item .env.example .env.local
npm install
npm run dev
```

Mở http://localhost:3000. Nếu trang chủ hiện **Backend API: OK** thì frontend đã nói chuyện được với backend.

## 3. Cổng dịch vụ

Cả nhóm dùng chung các cổng sau. Nếu máy bạn đang có chương trình khác chiếm cổng (ví dụ PostgreSQL cài sẵn), hãy tắt nó đi thay vì đổi cổng.

| Dịch vụ | Cổng | Cấu hình ở đâu |
|---|---|---|
| Frontend | 3000 | mặc định của `next dev` |
| Backend API | 5000 | `CulinaryBlog.API/Properties/launchSettings.json` |
| PostgreSQL | 5432 | `POSTGRES_PORT` trong `.env` |
| Redis (có mật khẩu) | 6379 | `REDIS_PORT` trong `.env` |
| MinIO API | 9000 | `MINIO_API_PORT` trong `.env` |
| MinIO Console | 9001 | `MINIO_CONSOLE_PORT` trong `.env` |
| Seq | 5341 | `SEQ_PORT` trong `.env` |
| Mailpit SMTP | 1025 | `MAILPIT_SMTP_PORT` trong `.env` |
| Mailpit UI | 8025 | `MAILPIT_UI_PORT` trong `.env` |

Khi thêm dịch vụ mới vào compose (storage, Mailpit, Seq…), nhớ bổ sung cổng vào bảng này trong cùng PR.

## 4. Cấu trúc thư mục

```
.
├── docker-compose.yml        PostgreSQL + Redis
├── .env.example
├── global.json               ghim .NET SDK
├── CLAUDE.md                 hướng dẫn cho Claude Code
├── docs/                     SRS, kế hoạch, OWNERSHIP, bảng mã lỗi
└── src/
    ├── Backend/
    │   ├── CulinaryBlog.Domain/          entity, không dùng thư viện ngoài
    │   ├── CulinaryBlog.Application/     use case (Features/<Module>), phần dùng chung (Common)
    │   ├── CulinaryBlog.Infrastructure/  EF Core (AppDbContext), cài đặt các interface
    │   ├── CulinaryBlog.API/             Program.cs, xử lý lỗi, OpenAPI, Endpoints/<Module>
    │   └── tests/                        unit, integration, architecture
    └── Frontend/
        ├── app/                          các trang
        ├── components/                   component dùng chung
        ├── features/<module>/            code riêng của từng module
        └── lib/api/client.ts             nơi duy nhất gọi backend
```

Mỗi thư mục module đều có README ghi người phụ trách và việc cần làm.

| Module | Phụ trách | Yêu cầu | Route |
|---|---|---|---|
| Auth | TV1 | FR-AUTH-001→007, FR-JOB-001 | `/api/v1/auth` |
| Recipes | TV2 | FR-RCP-002→007, 009, 010 | `/api/v1/recipes`, `/api/v1/me` |
| Categories | TV3 | FR-CAT-001→005 | `/api/v1/categories` |
| RecipeImages | TV3 | FR-RCP-008, FR-FILE, FR-JOB-002 | `/api/v1/recipes/{recipeId}/images` |
| RecipeSearch | TV4 | FR-RCP-001, FR-SRCH-001→004 | `/api/v1/recipes` (danh sách, `/search`) |
| Observability | TV4 | FR-OBS-001, 002 | `/health*` |

## 5. Thêm tính năng mới

**Một use case ở backend** gồm command/query, handler, validator và DTO, để chung trong một thư mục:

```
CulinaryBlog.Application/Features/Recipes/CreateRecipe/
├── CreateRecipeCommand.cs
├── CreateRecipeHandler.cs
├── CreateRecipeValidator.cs
└── RecipeDto.cs
```

Handler và validator được đăng ký tự động. Validator chạy trước handler; dữ liệu sai sẽ trả về lỗi 422.

**Endpoint** viết trong file `Endpoints/<Module>/<Module>Endpoints.cs` đã có sẵn. Không cần sửa `Program.cs`, vì các module được tự tìm thấy khi chạy:

```csharp
var recipes = api.MapGroup("/recipes").WithTags(Tag);
recipes.MapPost("/", async (CreateRecipeCommand command, ISender sender, CancellationToken ct) =>
{
    var recipe = await sender.Send(command, ct);
    return TypedResults.Created($"/api/v1/recipes/{recipe.Id}", recipe);
});
```

**Báo lỗi nghiệp vụ** bằng `NotFoundException`, `ConflictException`, `ForbiddenException` hoặc `ValidationException`, kèm mã lỗi có tiền tố module (ví dụ `RECIPE_SLUG_EXISTS`). Mã mới thì thêm vào [bảng mã lỗi](./docs/api/error-codes.md).

**Entity và migration:** entity kế thừa `BaseEntity` (đã có `Id`, `CreatedAt`, `UpdatedAt`, `Version`). Cấu hình EF đặt trong `CulinaryBlog.Infrastructure/<Module>/`, còn `DbSet` thêm vào `AppDbContext`. Tạo migration bằng:

```bash
cd src/Backend
dotnet ef migrations add Recipes_Init --project CulinaryBlog.Infrastructure --startup-project CulinaryBlog.API --output-dir Persistence/Migrations
dotnet ef database update --project CulinaryBlog.Infrastructure --startup-project CulinaryBlog.API
```

**Trang frontend:** các trang trong `app/` hiện là trang tạm ghi "Đang phát triển". Thay nội dung đó bằng component trong `features/<module>/`. Gọi API qua `apiClient`; khi lỗi, dựa vào `error.code` để hiện thông báo.

## 6. Làm việc nhóm

**Nhánh và commit**

- Không commit thẳng lên `main`. Mọi thay đổi đi qua Pull Request và cần một người khác review, cố gắng trong vòng 24 giờ (review chéo: TV1 ↔ TV2, TV3 ↔ TV4).
- Đặt tên nhánh theo dạng `mssv/<mã-việc>-<mô-tả>`, ví dụ `2312000/2.08-create-recipe`.
- Commit viết tiếng Anh theo Conventional Commits, scope là tên module: `feat(recipes): add publish endpoint`.
- Mỗi PR nên nhỏ (khoảng 400 dòng trở xuống) và chỉ làm một việc; mô tả ghi mã việc và mã FR.
- Mỗi ngày làm việc nên kéo `main` về nhánh mình một lần để tránh xung đột dồn lại.

**Sửa code của ai**

- Chỉ sửa trong phần của mình, theo [OWNERSHIP.md](./docs/OWNERSHIP.md).
- Các file dùng chung như `Program.cs`, `DependencyInjection.cs`, `Directory.Packages.props`, `AppDbContext`, `app/layout.tsx`, `next.config.ts`, `docker-compose.yml`, bảng mã lỗi: báo nhóm trước và tách PR riêng.
- Thêm thư viện thì ghim phiên bản (trong `Directory.Packages.props` hoặc `package.json`) và ghi rõ trong PR.

**Migration**

- Mỗi PR tối đa một migration, tên có tiền tố module: `Auth_Init`, `Recipes_Init`…
- Kéo `main` mới nhất ngay trước khi tạo migration. Không sửa migration đã merge. Nếu bị trùng snapshot, xóa migration của mình rồi tạo lại.

**API**

- Mọi endpoint nằm dưới `/api/v1`. Response trả thẳng dữ liệu, không bọc thêm lớp nào. Danh sách trả về `PagedResult` (`page` bắt đầu từ 1, `pageSize` mặc định 12, tối đa 50, sắp xếp kiểu `sort=-createdAt`).
- Lỗi trả theo chuẩn Problem Details, có thêm `code`: 400 khi request sai định dạng, 422 khi dữ liệu không hợp lệ hoặc vi phạm quy tắc, 409 khi trùng hoặc xung đột phiên bản.
- Tên trường JSON dùng camelCase, trùng với tên thuộc tính C#. Chỗ nào làm khác SRS v1.0 thì ghi vào bảng Change Request của SRS v1.1.

**Bảo mật**

- Không commit `.env`, mật khẩu, token. Secret đi qua biến môi trường hoặc `dotnet user-secrets`.
- Không ghi log mật khẩu, token hay dữ liệu nhạy cảm.

**Một việc được coi là xong khi:** đã merge và CI xanh; có test cho trường hợp thành công và ít nhất một trường hợp lỗi; lỗi trả đúng định dạng kèm `code`; API hiện đúng trên Scalar và màn hình chạy được với dữ liệu mẫu; chỗ nào khác SRS thì đã ghi lại.

## 7. Quy ước viết code

**Backend**

- Phụ thuộc đi một chiều: API → Infrastructure → Application → Domain. Application chỉ biết interface, không biết EF Core, Identity hay Hangfire. Architecture test sẽ báo nếu vi phạm.
- Endpoint chỉ nhận request, gửi qua MediatR rồi trả kết quả. Không đặt logic hay validation trong endpoint, và không dùng Controller.
- Validation viết bằng FluentValidation.
- Lỗi nghiệp vụ dùng exception có `code`. Không `throw new Exception(...)`, không nuốt lỗi, không viết thông báo tiếng Việt trong exception (frontend lo phần hiển thị).
- Mọi thao tác I/O đều `async`, nhận và truyền tiếp `CancellationToken`.
- Truy vấn chỉ đọc thì dùng `AsNoTracking()` và select thẳng ra DTO, chú ý tránh N+1.
- DTO là `record`; class không cần kế thừa thì để `sealed`; ưu tiên primary constructor. Map dữ liệu bằng tay, không dùng AutoMapper.
- Không để số hay chuỗi "thần kỳ" trong code: dùng hằng hoặc `IOptions<T>`.
- Build phải sạch warning. Chạy `dotnet format` trước khi commit.
- Tên test theo dạng `Method_Scenario_ExpectedResult`. Mỗi endpoint có ít nhất một test thành công và một test lỗi.

**Frontend**

- TypeScript strict, không dùng `any`.
- Mặc định là Server Component; chỉ thêm `"use client"` khi thật sự cần state hoặc sự kiện.
- Chỉ gọi API qua `lib/api/client.ts`.
- Code của module để trong `features/<module>/`; `components/` chỉ chứa phần dùng chung.
- Hiển thị lỗi theo `code`, không dựa vào chuỗi `detail`.
- Chỉ biến `NEXT_PUBLIC_*` mới dùng được ở trình duyệt; đừng đưa secret xuống client.
- Next.js 16 thay đổi khá nhiều so với bản cũ. Đọc `src/Frontend/AGENTS.md` và tài liệu trong `node_modules/next/dist/docs/` trước khi code.
- Chạy `npm run lint` và `npm run build` trước khi commit. Style bằng Tailwind.

**Chung**

- Hàm ngắn, làm một việc, tên nói rõ ý định. Xóa code chết thay vì comment lại.
- Comment để giải thích *vì sao*, không lặp lại code đang làm gì. Tên biến, hàm bằng tiếng Anh; comment và tài liệu có thể viết tiếng Việt.
- TODO phải ghi rõ người nhận: `TODO(TV2): ...`.

## 8. Tiến độ

Hạn chót là **Chủ nhật 01/11/2026**. Đến ngày này ứng dụng phải chạy trọn vẹn: đăng ký, đăng nhập, viết và quản lý công thức, duyệt danh mục, tìm kiếm, với giao diện hoàn chỉnh cho mọi trang. Sau 01/11 chỉ còn sửa lỗi nhỏ và làm phần mở rộng.

**Tuần này: 16/09 – 22/09** — chốt các quyết định, viết ADR và làm những việc nền đầu tiên của từng module.

| Thành viên | Việc có hạn trong tuần |
|---|---|
| TV1 | 1.01 – 1.04: xác nhận với giảng viên, SRS v1.1 + mẫu ADR, ADR xác thực, mã lỗi `AUTH_*` |
| TV2 | 2.01 – 2.02: ADR xóa dữ liệu/concurrency/vòng đời công thức, bảng mã lỗi chung và bảng đặt tên |
| TV3 | 3.01 – 3.02: thử image storage, ADR storage và ảnh |
| TV4 | 4.01 – 4.02: bảo vệ nhánh `main`, template PR, bảng Kanban, ADR cache/tìm kiếm/sitemap/NFR |

Các tuần sau được chia đều 7 ngày; lịch chi tiết nằm trong [kế hoạch tổng](./docs/KeHoach/00_KeHoach_TongThe.md) và [kế hoạch của từng người](./docs/KeHoach/). Mỗi người tự cập nhật tiến độ của mình.

## 9. Để nghiên cứu sau

- Cloudflare Tunnel để đưa bản demo từ máy nhóm ra Internet (TV3, việc 3.06).

## Thành viên

| MSSV | Họ và tên | Phụ trách | GitHub |
| :---: | :--- | :--- | :---: |
| 2312763 | <nobr>**Trần Lê Bảo Thư**</nobr> | Tìm kiếm, SEO, sitemap, observability (`FR-SRCH`, `FR-OBS`, `FR-JOB-003`) | [![GitHub](https://img.shields.io/badge/GitHub-Profile-181717?logo=github)](https://github.com/BaoThw05) |
| 2312793 | <nobr>**Nguyễn Ngọc Tuấn**</nobr> | Xác thực, người dùng, email chào mừng (`FR-AUTH`, `FR-JOB-001`) | [![GitHub](https://img.shields.io/badge/GitHub-Profile-181717?logo=github)](https://github.com/Liu-548) |
| 2312776 | <nobr>**Bùi Ngọc Toàn**</nobr> | Công thức, các bước, nguyên liệu (`FR-RCP`) | [![GitHub](https://img.shields.io/badge/GitHub-Profile-181717?logo=github)](https://github.com/2312776-beep) |
| 2312735 | <nobr>**Lương Đức Sang**</nobr> | Danh mục, lưu trữ file, xử lý ảnh (`FR-CAT`, `FR-FILE`, `FR-JOB-002`) | [![GitHub](https://img.shields.io/badge/GitHub-Profile-181717?logo=github)](https://github.com/zoronoa188) |

Bảng phân công gốc: [`docs/BangPhanCong.docx`](./docs/BangPhanCong.docx) · SRS bản PDF: [`docs/SRS_Culinary_Blog_v1.0.0.pdf`](./docs/SRS_Culinary_Blog_v1.0.0.pdf)
