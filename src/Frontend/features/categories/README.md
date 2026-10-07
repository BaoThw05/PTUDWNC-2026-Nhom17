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

## Trang công khai

`/categories` hiển thị lưới danh mục và số công thức Published. `/categories/[slug]` hiển thị công thức Published, 12 món mỗi trang, liên kết tới trang chi tiết công thức và trả 404 khi slug không tồn tại. Dữ liệu GET được cache bằng `fetch` với thời gian làm mới 3600/300 giây. Trang được render khi có request để build frontend không cần backend đang chạy; tham số `page` cũng được xử lý theo request.
