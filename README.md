# Culinary Blog

Blog chia sẻ công thức nấu ăn — đồ án môn Phát triển Ứng dụng Web Nâng cao của Nhóm 17.

Người dùng có thể đăng ký, viết và xuất bản công thức (kèm ảnh, nguyên liệu, các bước làm), duyệt theo danh mục và tìm kiếm tiếng Việt không dấu. Backend viết bằng .NET 10 (Minimal API, Clean Architecture, MediatR), frontend dùng Next.js 16, dữ liệu nằm trong PostgreSQL 16 và Redis 7.

**Mục tiêu của nhóm: đến Chủ nhật 01/11/2026 có một ứng dụng hoàn thiện** — đăng nhập chạy thật, giao diện đầy đủ cho mọi trang, web gần như hoàn chỉnh từ đầu đến cuối.

Tài liệu nằm trong `docs/`:

- [SRS v1.0.0](./docs/SRS_Culinary_Blog_v1.0.0.md) và [các quyết định đã chốt](./docs/SRS_Culinary_Blog_v1.0.0_GiaiPhap.md) (khi hai file nói khác nhau thì theo file quyết định)
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

**Cơ sở dữ liệu và Redis**

```bash
cp .env.example .env          # PowerShell: Copy-Item .env.example .env
docker compose up -d
docker compose ps             # đợi cả hai service báo (healthy)
```

**Backend**

```bash
cd src/Backend
dotnet build
dotnet test
dotnet run --project CulinaryBlog.API
```

API chạy ở http://localhost:5000. Mở http://localhost:5000/health để kiểm tra (trả về `Healthy`), tài liệu API ở http://localhost:5000/scalar.

Backend đọc connection string trong `appsettings.Development.json`, khớp sẵn với `.env.example`. Nếu máy bạn dùng cổng hoặc mật khẩu khác thì xem [mục 3.1](#31-máy-dùng-cổng-riêng).

**Frontend**

```bash
cd src/Frontend
cp .env.example .env.local    # PowerShell: Copy-Item .env.example .env.local
npm install
npm run dev
```

Mở http://localhost:3000. Nếu trang chủ hiện **Backend API: OK** thì frontend đã nói chuyện được với backend.

## 3. Cổng dịch vụ

Cả nhóm dùng chung các cổng sau. Code mẫu, cấu hình mặc định và tài liệu đều viết theo bộ cổng này, nên đừng đổi khi chưa bàn với nhóm.

| Dịch vụ | Cổng | Cấu hình ở đâu |
|---|---|---|
| Frontend | 3000 | mặc định của `next dev` |
| Backend API | 5000 | `CulinaryBlog.API/Properties/launchSettings.json` |
| PostgreSQL | 5432 | `POSTGRES_PORT` trong `.env` |
| Redis (có mật khẩu) | 6379 | `docker-compose.yml` |

Khi thêm dịch vụ mới vào compose (storage, Mailpit, Seq…), nhớ bổ sung cổng vào bảng này trong cùng PR.

### 3.1. Máy dùng cổng riêng

Nếu cổng chung trên máy bạn đã bị chương trình khác chiếm thì mới đổi, và ghi tên mình vào bảng dưới để mọi người biết khi giúp nhau sửa lỗi.

| Thành viên | Dịch vụ | Cổng | Lý do |
|---|---|---|---|
| TV1 — Nguyễn Ngọc Tuấn | PostgreSQL | 5433 | Máy đã cài sẵn PostgreSQL ở cổng 5432 |

Cách đổi (chỉ trên máy mình, không commit):

- **PostgreSQL:** sửa `POSTGRES_PORT` trong `.env` rồi chạy lại `docker compose up -d`. Vì .NET không đọc file `.env`, bạn cần báo cho backend biết bằng user-secrets:

  ```bash
  cd src/Backend
  dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5433;Database=culinary_blog;Username=culinary_admin;Password=<POSTGRES_PASSWORD>" --project CulinaryBlog.API
  ```

- **Backend:** chạy `dotnet run --project CulinaryBlog.API -- --urls http://localhost:<cổng>` và đặt `API_INTERNAL_URL` trong `src/Frontend/.env.local` cho khớp.
- **Frontend:** `npm run dev -- -p <cổng>`.
- **Redis:** cổng đang cố định trong compose. Nếu bị trùng, nhắn TV3 thêm biến `REDIS_PORT` thay vì tự sửa compose.

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

Nhóm tự chủ động sắp xếp thời gian, không có lịch họp cố định. Mỗi người tự cập nhật tiến độ trên bảng công việc. Nếu bị chặn hoặc thấy sắp trễ một mốc bàn giao, hãy báo ngay trong nhóm chat.

**Nhánh và commit**

- Không commit thẳng lên `main`. Mọi thay đổi đi qua Pull Request và cần một người khác review, cố gắng trong vòng 24 giờ (review chéo: TV1 ↔ TV2, TV3 ↔ TV4).
- Đặt tên nhánh theo dạng `tv<số>/<mã-việc>-<mô-tả>`, ví dụ `tv2/2.08-create-recipe`.
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

Bảng dưới liệt kê các việc có hạn trong từng tuần; chi tiết nằm trong [kế hoạch của từng người](./docs/KeHoach/). Mỗi người tự cập nhật cột cuối (phần trăm đã xong, việc nào đang trễ).

| Tuần | Ngày | Mốc | TV1 | TV2 | TV3 | TV4 | Tình hình |
|---|---|---|---|---|---|---|---|
| 1 | 17/09 – 20/09 | Chốt quyết định, viết ADR | 1.01–1.04 | 2.01–2.02 | 3.01–3.02 | 4.01–4.02 | |
| 2 | 21/09 – 27/09 | Dựng nền tảng | 1.05–1.06 | 2.03–2.06 | 3.03–3.06 | 4.03–4.04 | |
| 3 | 28/09 – 04/10 | Nền tảng chạy được (30/09) | 1.07–1.09 | 2.07–2.09 | 3.07–3.08 | 4.05–4.06 | |
| 4 | 05/10 – 11/10 | Xong API của cả 4 module | 1.10–1.15 | 2.10–2.15 | 3.09–3.12 | 4.07–4.10 | |
| 5 | 12/10 – 18/10 | Chạy được luồng đầy đủ trên giao diện | 1.16–1.18 | 2.16–2.19 | 3.13–3.15 | 4.11–4.13 | |
| 6 | 19/10 – 25/10 | Giao diện hoàn thiện, chốt tính năng | 1.19–1.21 | 2.20–2.22 | 3.16–3.19 | 4.14–4.17 | |
| 7 | 26/10 – 01/11 | Kiểm thử, tài liệu, **app hoàn thiện** | 1.22–1.26 | 2.23–2.27 | 3.20–3.24 | 4.18–4.22 | |

Những điểm bàn giao cần để ý vì trễ là kéo theo người khác: khung Next.js và API client (TV4, 25/09), compose đủ dịch vụ (TV3, 24/09), nền persistence (TV2, 25/09), entity Category (TV3, 26/09), entity Recipe (TV2, 27/09), CI (TV4, 27/09), đăng nhập và JWT (TV1, 29/09), Hangfire (TV3, 29/09), `ICacheInvalidator` (TV4, 30/09). Danh sách đầy đủ ở mục 6 của [kế hoạch tổng](./docs/KeHoach/00_KeHoach_TongThe.md).

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
