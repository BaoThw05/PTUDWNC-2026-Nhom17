# Culinary Blog — Frontend

Next.js 16 (App Router) + TypeScript + Tailwind CSS 4 + TanStack Query.

> Next.js 16 có nhiều thay đổi so với bản cũ. Đọc `AGENTS.md` và tài liệu trong `node_modules/next/dist/docs/` trước khi code.

## Chạy

```bash
cp .env.example .env.local   # PowerShell: Copy-Item .env.example .env.local
npm install
npm run dev                  # http://localhost:3000
```

Backend phải chạy ở `API_INTERNAL_URL` (mặc định `http://localhost:5000`). Trang chủ có khối **Backend API** để kiểm tra kết nối.

## Cấu trúc

| Đường dẫn | Nội dung |
|---|---|
| `app/` | Route (App Router). Trang chỉ ghép component, không chứa logic gọi API |
| `components/` | Component dùng chung: layout, `PlaceholderPage`, `HealthStatus`, provider |
| `features/<module>/` | Code của từng module (`api/`, `components/`, `hooks/`) — xem README trong từng thư mục |
| `lib/api/client.ts` | Điểm duy nhất gọi backend; lỗi được ném ra dưới dạng `ApiError` (có `status`, `code`, `problem`) |

## Gọi API

```ts
import { apiClient, ApiError } from "@/lib/api/client";

const recipes = await apiClient.get<PagedResult<RecipeSummary>>("/api/v1/recipes?page=1");
```

- Trình duyệt gọi `/api/*` cùng origin; `next.config.ts` rewrite sang backend (`/api/health` → `/health`).
- Code chạy phía server gọi thẳng `API_INTERNAL_URL`.
- Hiển thị lỗi theo `error.code`, không dùng chuỗi `detail`.
- Token: `lib/api/access-token.ts` (TV1 hoàn thiện ở việc 1.09).

## Kiểm tra trước khi commit

```bash
npm run lint
npm run build
```
