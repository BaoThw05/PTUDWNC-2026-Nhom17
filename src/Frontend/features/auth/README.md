# features/auth

- **Phụ trách:** TV1
- **FR:** FR-AUTH-001 → 007
- **Route:** `/auth/login`, `/auth/register`, `/profile`
- **Phạm vi:** Đăng ký, đăng nhập (Credentials + Google), Auth.js, hồ sơ cá nhân.

Cấu trúc gợi ý:

```
features/auth/
├── api/         # hàm gọi backend, chỉ dùng lib/api/client.ts
├── components/  # component riêng của module
└── hooks/       # hook TanStack Query (useXxx)
```

Trang trong `app/` chỉ ghép component từ thư mục này; component dùng chung cho nhiều module đặt ở `components/` gốc.
