# features/auth

- **Phụ trách:** TV1
- **FR:** FR-AUTH-001 → 007
- **Route:** `/auth/login`, `/auth/register`, `/profile` (và bảo vệ `/dashboard/*`)

Auth.js v5 theo mô hình BFF (ADR S-05): phiên nằm trong cookie HttpOnly đã mã hóa, bên trong giữ access token (15 phút) và refresh token của backend. Token được làm mới trong callback `jwt` trước khi hết hạn 1 phút.

## Cấu trúc

| File | Nội dung |
|---|---|
| `auth.ts` | Cấu hình Auth.js: Credentials, Google (khi có `AUTH_GOOGLE_ID`), refresh token, thu hồi khi đăng xuất |
| `server.ts` | `getServerAccessToken()`, `requireSession()` cho Server Component |
| `api/backend.ts` | Gọi `/api/v1/auth/*` từ phía server (trong Auth.js) |
| `api/client.ts` | Gọi từ trình duyệt: đăng ký, sửa hồ sơ |
| `components/` | Form đăng nhập/đăng ký/hồ sơ, `UserMenu`, `AuthSessionProvider` |
| `errors.ts` | Thông báo tiếng Việt theo mã lỗi backend |
| `/proxy.ts` (gốc frontend) | Chưa đăng nhập mà vào `/dashboard/*`, `/profile` → chuyển về `/auth/login?callbackUrl=...` |

## Dùng ở module khác

```tsx
// Client Component: apiClient tự gắn token của phiên hiện tại.
const recipes = await apiClient.get<PagedResult<Recipe>>("/api/v1/me/recipes");

// Server Component: lấy token rồi truyền vào.
import { requireSession } from "@/features/auth/server";
const session = await requireSession("/dashboard/recipes");
const recipes = await apiClient.get("/api/v1/me/recipes", { accessToken: session.accessToken });

// Thông tin người dùng: useSession() (client) hoặc auth() (server).
session.user.id; session.user.displayName; session.user.roles; // ["Author"] hoặc ["Admin", "Author"]
```

Tài khoản mẫu khi chạy dev: `author1@culinaryblog.test` / `Author@12345` (xem `src/Backend/CulinaryBlog.API/Endpoints/Auth/README.md`).

## Biến môi trường

Xem `.env.example`: `AUTH_SECRET` (bắt buộc ở production), `AUTH_GOOGLE_ID`, `AUTH_GOOGLE_SECRET`.
