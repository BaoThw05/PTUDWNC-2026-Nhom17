# features/categories

- **Phụ trách:** TV3
- **FR:** FR-CAT-001 → 005
- **Route:** `/categories`, `/categories/[slug]`, `/dashboard/categories`
- **Phạm vi:** Danh sách/chi tiết danh mục, trang quản lý danh mục (Admin).

Cấu trúc gợi ý:

```
features/categories/
├── api/         # hàm gọi backend, chỉ dùng lib/api/client.ts
├── components/  # component riêng của module
└── hooks/       # hook TanStack Query (useXxx)
```

Trang trong `app/` chỉ ghép component từ thư mục này; component dùng chung cho nhiều module đặt ở `components/` gốc.
