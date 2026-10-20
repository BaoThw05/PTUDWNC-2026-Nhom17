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
## Trang công khai

`/categories` hiển thị lưới danh mục và số công thức Published. `/categories/[slug]` hiển thị công thức Published, 12 món mỗi trang, liên kết tới trang chi tiết công thức và trả 404 khi slug không tồn tại. Các trang tiếp theo có URL `/categories/[slug]/page/[n]`; liên kết cũ `?page=n` được chuyển hướng. Route dùng ISR theo từng URL: danh sách làm mới sau 3600 giây, chi tiết sau 300 giây. `generateStaticParams` trả danh sách rỗng để trang được tạo khi truy cập lần đầu và build không cần backend đang chạy.
