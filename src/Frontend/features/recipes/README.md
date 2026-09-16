# features/recipes

- **Phụ trách:** TV2
- **FR:** FR-RCP-002 → 007, 009, 010
- **Route:** `/recipes/[slug]`, `/dashboard`, `/dashboard/recipes/**`
- **Phạm vi:** Trang chi tiết, dashboard, wizard tạo/sửa công thức, thùng rác.

Cấu trúc gợi ý:

```
features/recipes/
├── api/         # hàm gọi backend, chỉ dùng lib/api/client.ts
├── components/  # component riêng của module
└── hooks/       # hook TanStack Query (useXxx)
```

Trang trong `app/` chỉ ghép component từ thư mục này; component dùng chung cho nhiều module đặt ở `components/` gốc.
