# Culinary Blog – Blog Ẩm thực và Nấu ăn

> **Culinary Blog** là nền tảng web cho phép người dùng khám phá, chia sẻ và quản lý công thức nấu ăn. Hệ thống theo kiến trúc **API-Driven**: Backend .NET 10 (Minimal APIs, Clean Architecture, CQRS với MediatR) và Frontend Next.js 16 (App Router) tách rời, dùng PostgreSQL 16 và Redis 7.

Tài liệu đặc tả: [`docs/SRS_Culinary_Blog_v1.0.0.md`](./docs/SRS_Culinary_Blog_v1.0.0.md) · quyết định chốt: [`docs/SRS_Culinary_Blog_v1.0.0_GiaiPhap.md`](./docs/SRS_Culinary_Blog_v1.0.0_GiaiPhap.md) (thắng SRS khi mâu thuẫn) · kế hoạch: [`docs/KeHoach/`](./docs/KeHoach/) · phân vùng sở hữu: [`docs/OWNERSHIP.md`](./docs/OWNERSHIP.md) · mã lỗi: [`docs/api/error-codes.md`](./docs/api/error-codes.md).

## Mục lục

1. [Yêu cầu cài đặt](#1-yêu-cầu-cài-đặt)
2. [Chạy từng bước](#2-chạy-từng-bước)
3. [Cổng dịch vụ](#3-cổng-dịch-vụ)
4. [Cấu trúc thư mục](#4-cấu-trúc-thư-mục)
5. [Thêm module, endpoint, trang mới](#5-thêm-module-endpoint-trang-mới)
6. [Quy tắc làm việc nhóm](#6-quy-tắc-làm-việc-nhóm)
7. [Clean code](#7-clean-code)
8. [Phân chia công việc theo tuần](#8-phân-chia-công-việc-theo-tuần)
9. [Nghiên cứu sau](#9-nghiên-cứu-sau)
10. [Thành viên](#thành-viên)

---

## 1. Yêu cầu cài đặt

| Công cụ | Phiên bản |
|---|---|
| .NET SDK | 10.0.x |
| Node.js | ≥ 20.9 (khuyến nghị 22 LTS trở lên) |
| npm | ≥ 10 |
| Docker Desktop | bản mới, có Docker Compose v2 (Windows: bật WSL2 và **mở Docker Desktop trước khi chạy compose**) |
| Git | ≥ 2.40 |
| dotnet-ef (khi cần tạo migration) | `dotnet tool install --global dotnet-ef` |

## 2. Chạy từng bước

Lệnh dưới đây chạy được trên PowerShell và bash, tính từ thư mục gốc repo.

**Bước 1 — Hạ tầng (PostgreSQL + Redis)**

```bash
cp .env.example .env          # PowerShell: Copy-Item .env.example .env
docker compose up -d
docker compose ps             # cả hai service phải ở trạng thái (healthy)
```

**Bước 2 — Backend API**

```bash
cd src/Backend
dotnet build
dotnet test
dotnet run --project CulinaryBlog.API   # http://localhost:5000
```

- Kiểm tra: `http://localhost:5000/health` trả `Healthy`; tài liệu API ở `http://localhost:5000/scalar`.
- Connection string mặc định (trong `appsettings.Development.json`) dùng **cổng chung 5432** và mật khẩu mặc định của `.env.example`. Máy nào dùng cổng riêng hoặc mật khẩu khác thì làm theo [mục 3.1](#31-máy-dùng-cổng-riêng).

**Bước 3 — Frontend**

```bash
cd src/Frontend
cp .env.example .env.local    # PowerShell: Copy-Item .env.example .env.local
npm install
npm run dev                   # http://localhost:3000
```

Trang chủ có khối **Backend API: OK** khi frontend gọi được backend qua `/api/health`.

## 3. Cổng dịch vụ

**Cả nhóm dùng chung bộ cổng dưới đây.** Tài liệu, code mẫu và cấu hình mặc định trong repo đều viết theo các cổng này; không đổi cổng chung khi chưa thống nhất với nhóm.

| Dịch vụ | Cổng chung | Địa chỉ | Cấu hình ở | Ghi chú |
|---|---|---|---|---|
| Frontend (Next.js) | **3000** | http://localhost:3000 | mặc định của `next dev` | `/api/*` được rewrite sang backend |
| Backend API | **5000** | http://localhost:5000 | `CulinaryBlog.API/Properties/launchSettings.json` | `/api/v1/*`, `/health`, `/scalar` (Dev) |
| PostgreSQL | **5432** | localhost:5432 | `POSTGRES_PORT` trong `.env` | DB `culinary_blog`, user `culinary_admin` |
| Redis | **6379** | localhost:6379 | cố định trong `docker-compose.yml` | Có mật khẩu `REDIS_PASSWORD` |

TV3 sẽ bổ sung storage, Mailpit, Seq vào compose (việc 3.03); khi thêm dịch vụ mới, bổ sung cổng vào bảng này trong cùng PR.

### 3.1. Máy dùng cổng riêng

Chỉ dùng cổng khác khi cổng chung trên máy đã bị chương trình khác chiếm. **Ai dùng cổng riêng phải ghi vào bảng dưới** (PR sửa README) để cả nhóm biết khi hỗ trợ nhau.

| Thành viên | Dịch vụ | Cổng riêng | Lý do |
|---|---|---|---|
| TV1 — Nguyễn Ngọc Tuấn | PostgreSQL | **5433** | Cổng 5432 đã bị PostgreSQL cài sẵn trên máy chiếm |

Cách cấu hình (chỉ trên máy của mình, **không commit**):

1. **PostgreSQL** — trong `.env` ở gốc repo đặt `POSTGRES_PORT=5433` (hoặc cổng khác), rồi `docker compose up -d`. .NET không đọc file `.env`, nên ghi đè connection string cho API bằng user-secrets:

   ```bash
   cd src/Backend
   dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5433;Database=culinary_blog;Username=culinary_admin;Password=<POSTGRES_PASSWORD>" --project CulinaryBlog.API
   ```

2. **Backend API** — không sửa `launchSettings.json`; chạy `dotnet run --project CulinaryBlog.API -- --urls http://localhost:<cổng>` và đặt `API_INTERNAL_URL=http://localhost:<cổng>` trong `src/Frontend/.env.local`.
3. **Frontend** — `npm run dev -- -p <cổng>`.
4. **Redis** — cổng 6379 đang cố định trong compose; nếu bị chiếm, báo TV3 để thêm biến `REDIS_PORT` thay vì sửa compose trên máy mình.

Kiểm tra lại: `docker compose ps` (cột PORTS), `http://localhost:<cổng API>/health`, khối **Backend API: OK** trên trang chủ.

## 4. Cấu trúc thư mục

```
.
├── docker-compose.yml            # PostgreSQL + Redis
├── .env.example
├── docs/
│   ├── SRS_Culinary_Blog_v1.0.0*.md
│   ├── KeHoach/                  # kế hoạch tổng + 4 kế hoạch cá nhân
│   ├── OWNERSHIP.md              # thư mục → người phụ trách
│   └── api/error-codes.md        # bảng mã lỗi
└── src/
    ├── Backend/
    │   ├── Directory.Build.props / Directory.Packages.props
    │   ├── CulinaryBlog.Domain/          # Common/BaseEntity + thư mục module (không NuGet)
    │   ├── CulinaryBlog.Application/     # Common/ (PagedResult, exceptions, ErrorCodes, ValidationBehavior)
    │   │                                  # Abstractions/, Features/<Module>/
    │   ├── CulinaryBlog.Infrastructure/  # Persistence/AppDbContext + thư mục module
    │   ├── CulinaryBlog.API/             # Program.cs, ErrorHandling/, OpenApi/, Endpoints/<Module>/
    │   └── tests/                        # UnitTests, IntegrationTests, ArchitectureTests
    └── Frontend/
        ├── app/                          # route (placeholder theo người phụ trách)
        ├── components/                   # layout, PlaceholderPage, HealthStatus, providers
        ├── features/<module>/            # code từng module
        └── lib/api/                      # client.ts (điểm duy nhất gọi backend)
```

Module và người phụ trách:

| Module | Phụ trách | FR | Nhóm route |
|---|---|---|---|
| Auth | TV1 | FR-AUTH-001→007, FR-JOB-001 | `/api/v1/auth` |
| Recipes | TV2 | FR-RCP-002→007, 009, 010 | `/api/v1/recipes`, `/api/v1/me` |
| Categories | TV3 | FR-CAT-001→005 | `/api/v1/categories` |
| RecipeImages | TV3 | FR-RCP-008, FR-FILE, FR-JOB-002 | `/api/v1/recipes/{recipeId}/images` |
| RecipeSearch | TV4 | FR-RCP-001, FR-SRCH-001→004 | `/api/v1/recipes` (danh sách + `/search`) |
| Observability | TV4 | FR-OBS-001, 002 | `/health*` |

## 5. Thêm module, endpoint, trang mới

**Use case backend** (ví dụ tạo công thức):

```
CulinaryBlog.Application/Features/Recipes/CreateRecipe/
├── CreateRecipeCommand.cs      # record : IRequest<RecipeDto>
├── CreateRecipeHandler.cs      # IRequestHandler<,>, nhận CancellationToken
├── CreateRecipeValidator.cs    # AbstractValidator<CreateRecipeCommand> — tự đăng ký
└── RecipeDto.cs
```

Handler và validator được `AddApplication()` tự quét; `ValidationBehavior` chạy validator trước handler và ném `ValidationException` (→ 422).

**Endpoint:** thêm vào file `Endpoints/<Module>/<Module>Endpoints.cs` của module (đã có sẵn nhóm route). Lớp cài `IEndpointModule` được dò tự động, **không sửa `Program.cs`**:

```csharp
var recipes = api.MapGroup("/recipes").WithTags(Tag);
recipes.MapPost("/", async (CreateRecipeCommand command, ISender sender, CancellationToken ct) =>
{
    var recipe = await sender.Send(command, ct);
    return TypedResults.Created($"/api/v1/recipes/{recipe.Id}", recipe);
});
```

**Lỗi nghiệp vụ:** ném `NotFoundException`, `ConflictException`, `ForbiddenException`, `ValidationException` (hoặc lớp con của `AppException`) kèm `code` có tiền tố module, rồi thêm mã vào `docs/api/error-codes.md`. Cần HTTP status khác → thêm nhánh trong `GlobalExceptionHandler` (file dùng chung).

**Entity + migration:** entity kế thừa `BaseEntity` (đã có `Id`, `CreatedAt`, `UpdatedAt`, `Version` ↔ `xmin`); cấu hình `IEntityTypeConfiguration<T>` đặt trong `CulinaryBlog.Infrastructure/<Module>/` (tự nạp); thêm `DbSet` vào `AppDbContext` (file dùng chung). Tạo migration:

```bash
cd src/Backend
dotnet ef migrations add Recipes_Init --project CulinaryBlog.Infrastructure --startup-project CulinaryBlog.API --output-dir Persistence/Migrations
dotnet ef database update --project CulinaryBlog.Infrastructure --startup-project CulinaryBlog.API
```

**Trang frontend:** thay nội dung `app/<route>/page.tsx` (đang là `PlaceholderPage`) bằng component trong `features/<module>/`. Gọi API bằng `apiClient` trong `lib/api/client.ts`; bắt `ApiError` và hiển thị theo `error.code`.

## 6. Quy tắc làm việc nhóm

**Nhánh & commit**
- Không commit thẳng lên nhánh chính (`main`); mọi thay đổi đi qua PR, CI xanh (khi TV4 bật CI), **1 người review trong 24 giờ** (cặp chéo TV1↔TV2, TV3↔TV4).
- Tên nhánh: `tv<số>/<mã-việc>-<mô-tả-ngắn>`, ví dụ `tv1/1.09-register`, `tv2/2.08-create-recipe`.
- Commit theo Conventional Commits, tiếng Anh: `feat(recipes): ...`, `fix(auth): ...`, `chore:`, `docs:`, `test:`, `refactor:`. Scope = tên module.
- PR nhỏ (≤ ~400 dòng thay đổi nếu được), một mục đích; mô tả ghi mã việc (`2.08`) và mã FR.
- Cập nhật nhánh từ nhánh chính ít nhất mỗi ngày làm việc; tự giải quyết xung đột trong phần của mình.

**Phân vùng sở hữu**
- Chỉ sửa thư mục của mình (theo [`docs/OWNERSHIP.md`](./docs/OWNERSHIP.md)).
- File dùng chung (`Program.cs`, các `DependencyInjection.cs`, `Directory.Packages.props`, `AppDbContext`, `app/layout.tsx`, `next.config.ts`, `docker-compose.yml`, `docs/api/error-codes.md`): báo nhóm trước, PR riêng, ghi rõ trong mô tả.
- Thêm gói NuGet/npm: thêm phiên bản vào `Directory.Packages.props` / `package.json` (ghim bản), nói trong PR.

**Migration**
- Mỗi PR tối đa 1 migration, tên có tiền tố module: `Auth_AddRefreshTokens`, `Recipes_Init`…
- Kéo nhánh chính mới nhất **ngay trước** khi tạo migration; không sửa migration đã merge; trùng snapshot thì xóa migration của mình và tạo lại.

**Hợp đồng API (S-10)**
- Tiền tố `/api/v1`; `/health*`, `/scalar` là endpoint vận hành.
- Không vỏ bọc response; danh sách trả `PagedResult<T>`; `page` ≥ 1, `pageSize` mặc định 12, tối đa 50; `sort=-field` theo whitelist.
- Lỗi = Problem Details + `code` (SCREAMING_SNAKE_CASE, tiền tố module); 400 sai định dạng, 422 validation/vi phạm nghiệp vụ, 409 trùng/xung đột phiên bản.
- Mã lỗi mới phải thêm vào `docs/api/error-codes.md` trong cùng PR.
- Tên JSON camelCase trùng tên thuộc tính C# (bảng đặt tên S-10e).
- Khác SRS v1.0 → ghi vào bảng Change Request của SRS v1.1.

**Bảo mật**
- Không commit `.env`, secret, token, mật khẩu thật; secret đi qua biến môi trường hoặc `dotnet user-secrets`.
- Không log mật khẩu, token, body chứa thông tin nhạy cảm.

**Definition of Done:** đã merge, CI xanh · có test nhánh thành công + ≥ 1 nhánh lỗi · lỗi đúng Problem Details + `code` · hiển thị đúng trên Scalar, màn hình chạy với dữ liệu seed · khác SRS thì đã ghi CR.

**Họp:** Chủ nhật 20:30 (30 phút, chốt mốc tuần) · Thứ tư 21:00 (15 phút: đã xong gì, sắp làm gì, bị chặn bởi ai).

## 7. Clean code

**Backend (.NET)**
- Hướng phụ thuộc: API → Infrastructure → Application → Domain. Domain không NuGet; Application không biết EF Core/Identity/Hangfire (chỉ interface). Architecture test phải xanh.
- Tổ chức theo tính năng: `Features/<Module>/<UseCase>/` chứa `XxxCommand|Query`, `XxxHandler`, `XxxValidator`, DTO của use case đó. Mỗi file một kiểu.
- Endpoint mỏng: chỉ nhận request → `ISender.Send` → trả `TypedResults`. Không logic nghiệp vụ, không validation trong endpoint (CONS-008). Không dùng MVC Controller.
- Validation chỉ bằng FluentValidation qua `ValidationBehavior`.
- Lỗi nghiệp vụ ném exception kế thừa `AppException` kèm `code`; không `throw new Exception(...)`; không bắt exception rồi nuốt; không hardcode chuỗi tiếng Việt trong exception.
- Mọi I/O là `async` + nhận `CancellationToken` và truyền xuống; hậu tố `Async`.
- Query đọc dùng `AsNoTracking()` + projection sang DTO; tránh N+1 (`AsSplitQuery()` khi cần).
- DTO dùng `record`; class không kế thừa thì `sealed`; ưu tiên primary constructor cho DI.
- Map thủ công (không dùng AutoMapper ≥ 15).
- Đặt tên: PascalCase cho kiểu/method/property, `_camelCase` cho field private, `camelCase` cho biến/tham số; interface bắt đầu bằng `I`. Namespace file-scoped.
- Không magic number/string: dùng hằng hoặc options (`IOptions<T>`) từ appsettings.
- Build không warning (CI bật `TreatWarningsAsErrors`); chạy `dotnet format` trước khi commit.
- Tên test: `Method_Scenario_ExpectedResult`; mỗi endpoint ≥ 1 test thành công + 1 test lỗi.

**Frontend (Next.js)**
- TypeScript strict, không dùng `any` (dùng `unknown` rồi thu hẹp).
- Server Component mặc định; chỉ `"use client"` khi cần state/sự kiện/hook trình duyệt.
- Gọi API **chỉ** qua `lib/api/client.ts`; không `fetch` rải rác; kiểu dữ liệu lấy từ OpenAPI khi TV4 có script sinh kiểu.
- Code module để trong `features/<module>/` (components, hooks, api); `components/` gốc chỉ chứa phần dùng chung.
- Component PascalCase, một component chính mỗi file; hook bắt đầu bằng `use`.
- Hiển thị lỗi theo `code` của Problem Details, không theo chuỗi `detail`.
- Chỉ biến `NEXT_PUBLIC_*` mới được dùng ở trình duyệt; không đưa secret xuống client.
- Next.js 16 có breaking changes: đọc `node_modules/next/dist/docs/` trước khi code (xem `src/Frontend/AGENTS.md`).
- Chạy `npm run lint` trước khi commit; Tailwind cho style, tránh CSS inline.

**Chung**
- Hàm ngắn, làm một việc; tên nói rõ ý định; xóa code chết và code bị comment-out.
- Comment giải thích **vì sao**, không nhắc lại code làm gì. Tên định danh tiếng Anh; comment/tài liệu được dùng tiếng Việt.
- Không để `TODO` không có chủ: viết `TODO(TVx): ...`.

## 8. Phân chia công việc theo tuần

Mã việc chi tiết trong [`docs/KeHoach/`](./docs/KeHoach/). Mỗi ô ghi các việc có **hạn** trong tuần đó. Cột cuối cập nhật trong buổi họp Chủ nhật (% Done + mã việc đang trễ).

| Tuần | Ngày | Mốc | TV1 | TV2 | TV3 | TV4 | Trạng thái / ghi chú |
|---|---|---|---|---|---|---|---|
| 1 | 17/09 – 20/09 | **M0** — chốt quyết định, ADR | 1.01 – 1.04 | 2.01 – 2.02 | 3.01 – 3.02 | 4.01 – 4.02 | |
| 2 | 21/09 – 27/09 | G1 — nền tảng | 1.05 – 1.06 | 2.03 – 2.06 | 3.03 – 3.06 | 4.03 – 4.04 | |
| 3 | 28/09 – 04/10 | **M1** (30/09) — nền chạy | 1.07 – 1.09 | 2.07 – 2.09 | 3.07 – 3.08 | 4.05 – 4.06 | |
| 4 | 05/10 – 11/10 | **M2** — backend base | 1.10 – 1.15 | 2.10 – 2.15 | 3.09 – 3.12 | 4.07 – 4.10 | |
| 5 | 12/10 – 18/10 | **M3** — frontend base | 1.16 – 1.18 | 2.16 – 2.19 | 3.13 – 3.15 | 4.11 – 4.13 | |
| 6 | 19/10 – 25/10 | **M4** — tích hợp, chốt tính năng | 1.19 – 1.21 | 2.20 – 2.22 | 3.16 – 3.19 | 4.14 – 4.17 | |
| 7 | 26/10 – 01/11 | **M5, M6**, hạn 01/11 | 1.22 – 1.26 | 2.23 – 2.27 | 3.20 – 3.24 | 4.18 – 4.22 | |

Bàn giao chéo quan trọng: khung Next.js + API client (TV4, 25/09) · compose đủ service (TV3, 24/09) · `AuditInterceptor`/`IUnitOfWork` (TV2, 25/09) · entity `Category` (TV3, 26/09) · entity `Recipe` (TV2, 27/09) · CI (TV4, 27/09) · JWT + login (TV1, 29/09) · Hangfire + `IBackgroundJobService` (TV3, 29/09) · `ICacheInvalidator` (TV4, 30/09). Danh sách đầy đủ ở mục 6 của `docs/KeHoach/00_KeHoach_TongThe.md`.

## 9. Nghiên cứu sau

- **Cloudflare Tunnel** — đưa bản demo từ máy nhóm ra Internet (S-09, việc 3.06 của TV3). Chỉ ghi nhận, chưa làm trong khung gốc.

---

## Thành viên

| MSSV | Họ và Tên | Vai trò & Mô-đun phụ trách | Hồ sơ Git |
| :---: | :--- | :--- | :---: |
| 2312763 | <nobr>**Trần Lê Bảo Thư**</nobr> | Full-Stack: Module Tìm kiếm Full-Text, SEO, Sitemap & Observability (`FR-SRCH`, `FR-OBS`, `FR-JOB-003`) | [![Git Profile](https://img.shields.io/badge/GitHub-Profile-181717?logo=github)](https://github.com/BaoThw05) |
| 2312793 | <nobr>**Nguyễn Ngọc Tuấn**</nobr> | Full-Stack: Module Xác thực, Người dùng & Welcome Email (`FR-AUTH`, `FR-JOB-001`) | [![Git Profile](https://img.shields.io/badge/GitHub-Profile-181717?logo=github)](https://github.com/Liu-548) |
| 2312776 | <nobr>**Bùi Ngọc Toàn**</nobr> | Full-Stack: Module Công thức Nấu ăn Cốt lõi, Bước & Nguyên liệu (`FR-RCP`) | [![Git Profile](https://img.shields.io/badge/GitHub-Profile-181717?logo=github)](https://github.com/2312776-beep) |
| 2312735 | <nobr>**Lương Đức Sang**</nobr> | Full-Stack: Module Danh mục, Quản lý Tệp tin & Job Resize Ảnh (`FR-CAT`, `FR-FILE`, `FR-JOB-002`) | [![Git Profile](https://img.shields.io/badge/GitHub-Profile-181717?logo=github)](https://github.com/zoronoa188) |

- Bảng phân công gốc: [`docs/BangPhanCong.docx`](./docs/BangPhanCong.docx) · SRS bản PDF: [`docs/SRS_Culinary_Blog_v1.0.0.pdf`](./docs/SRS_Culinary_Blog_v1.0.0.pdf)
