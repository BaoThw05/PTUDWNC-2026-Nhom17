# features/images

- **Phụ trách:** TV3
- **FR:** FR-RCP-008, FR-FILE
- **Route:** (dùng trong wizard của TV2)
- **Phạm vi:** Component upload ảnh, chọn ảnh chính, hiển thị biến thể ảnh.

Cấu trúc gợi ý:

```
features/images/
├── api/         # hàm gọi backend, chỉ dùng lib/api/client.ts
├── components/  # component riêng của module
└── hooks/       # hook TanStack Query (useXxx)
```

Trang trong `app/` chỉ ghép component từ thư mục này; component dùng chung cho nhiều module đặt ở `components/` gốc.
