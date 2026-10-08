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

## Trang quản trị

`/dashboard/categories` kiểm tra phiên đăng nhập và vai trò Admin ở server. `CategoryAdmin` dùng API danh mục để tải danh sách, tạo, sửa, xóa; backend cũng yêu cầu policy Admin cho ba lệnh ghi. Form giữ `orderIndex` khi sửa, hiển thị lỗi 409 cho tên trùng và danh mục còn công thức. `api/client.ts` dùng `lib/api/client.ts` để gắn access token.
