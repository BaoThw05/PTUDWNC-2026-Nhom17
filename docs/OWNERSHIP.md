# Phân vùng sở hữu mã nguồn

> Khung gốc (base skeleton) do **TV1 dựng; người phụ trách từng phần tiếp quản và mở rộng**.
> Chỉ sửa thư mục của mình. Cần sửa phần của người khác → nhắn người đó trước hoặc nhờ họ sửa.

## Thành viên

| Mã | Thành viên | Module |
|---|---|---|
| TV1 | Nguyễn Ngọc Tuấn | Auth & Profile, Welcome Email; tài liệu SRS v1.1/ADR |
| TV2 | Bùi Ngọc Toàn | Recipe Core; nền backend (persistence, xử lý lỗi, hợp đồng API) |
| TV3 | Lương Đức Sang | Categories, File/Storage, Ảnh; hạ tầng (compose, Hangfire, Nginx, Tunnel) |
| TV4 | Trần Lê Bảo Thư | Danh sách/Tìm kiếm, SEO, Observability; nền frontend, CI |

## Backend (`src/Backend/`)

| Đường dẫn | Phụ trách |
|---|---|
| `CulinaryBlog.*/Auth/`, `CulinaryBlog.Application/Features/Auth/`, `CulinaryBlog.API/Endpoints/Auth/` | TV1 |
| `CulinaryBlog.*/Recipes/`, `CulinaryBlog.Application/Features/Recipes/`, `CulinaryBlog.API/Endpoints/Recipes/` | TV2 |
| `CulinaryBlog.*/Categories/`, `…/RecipeImages/` (cả 4 tầng) | TV3 |
| `CulinaryBlog.*/RecipeSearch/`, `…/Observability/` (các tầng có thư mục) | TV4 |
| `CulinaryBlog.Domain/Common/`, `CulinaryBlog.Application/Common/`, `CulinaryBlog.Infrastructure/Persistence/` | TV2 |
| `CulinaryBlog.API/ErrorHandling/` | TV2 |
| `CulinaryBlog.API/OpenApi/`, `CulinaryBlog.API/Endpoints/*.cs` (cơ chế dò module) | TV4 |
| `CulinaryBlog.Application/Abstractions/` | Mỗi interface thuộc người ghi trong README của thư mục |
| `tests/CulinaryBlog.UnitTests/`, `tests/CulinaryBlog.IntegrationTests/` | Mỗi người một thư mục con theo module |
| `tests/CulinaryBlog.ArchitectureTests/` | TV2 |

## Frontend (`src/Frontend/`)

| Đường dẫn | Phụ trách |
|---|---|
| `app/auth/`, `app/profile/`, `features/auth/`, `lib/api/access-token.ts` | TV1 |
| `app/recipes/[slug]/`, `app/dashboard/page.tsx`, `app/dashboard/recipes/`, `features/recipes/` | TV2 |
| `app/categories/`, `app/dashboard/categories/`, `features/categories/`, `features/images/` | TV3 |
| `app/page.tsx`, `app/recipes/page.tsx`, `app/search/`, `features/search/` | TV4 |
| `components/`, `lib/api/client.ts`, `app/not-found.tsx`, `app/error.tsx`, cấu hình ESLint | TV4 |

## Hạ tầng & tài liệu

| Đường dẫn | Phụ trách |
|---|---|
| `docker-compose.yml`, `.env.example`, `docker/` | TV3 |
| `.github/` (CI, template PR), CODEOWNERS | TV4 |
| `docs/SRS_v1.1.md`, `docs/ADR/` | TV1 (mỗi người viết ADR phần mình) |
| `docs/api/error-codes.md` | TV2 (mỗi người điền mục của module mình) |
| `docs/KeHoach/` | Mỗi người cập nhật file của mình |

## File dùng chung — báo nhóm trước khi sửa

Sửa các file dưới đây phải **báo nhóm trước**, làm **PR riêng** và ghi rõ trong mô tả PR:

- `src/Backend/CulinaryBlog.API/Program.cs`
- `src/Backend/CulinaryBlog.Application/DependencyInjection.cs`, `src/Backend/CulinaryBlog.Infrastructure/DependencyInjection.cs`
- `src/Backend/Directory.Packages.props`, `src/Backend/Directory.Build.props`
- `src/Backend/CulinaryBlog.Infrastructure/Persistence/AppDbContext.cs`
- `src/Backend/CulinaryBlog.API/ErrorHandling/GlobalExceptionHandler.cs` (thêm ánh xạ exception của module)
- `src/Frontend/app/layout.tsx`, `src/Frontend/next.config.ts`, `src/Frontend/package.json`
- `docker-compose.yml`, `.env.example`
- `docs/api/error-codes.md`
- `README.md` gốc
