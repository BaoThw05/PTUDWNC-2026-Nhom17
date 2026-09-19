# features/search

- **Phụ trách:** TV4
- **FR:** FR-RCP-001, FR-SRCH-001 → 004
- **Route:** `/`, `/recipes`, `/search`
- **Phạm vi:** Trang chủ, danh sách công thức, tìm kiếm, bộ lọc, SEO/JSON-LD, sitemap.

Cấu trúc gợi ý:

```
features/search/
├── api/         # hàm gọi backend, chỉ dùng lib/api/client.ts
├── components/  # component riêng của module
└── hooks/       # hook TanStack Query (useXxx)
```

Trang trong `app/` chỉ ghép component từ thư mục này; component dùng chung cho nhiều module đặt ở `components/` gốc.
