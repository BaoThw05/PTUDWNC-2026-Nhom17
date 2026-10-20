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

## Dùng trong wizard và trang sửa

Sau khi tạo recipe và nhận `recipeId`, TV2 có thể render:

```tsx
import { RecipeImageManager } from "@/features/images/components/RecipeImageManager";

<RecipeImageManager recipeId={recipeId} />
```

Component tự tải ảnh đã lưu, hỗ trợ kéo thả nhiều file JPEG/PNG/WebP, tiến trình từng file, đổi ảnh chính, sửa alt text, đổi thứ tự và xóa. Có thể truyền `initialImages` để hiển thị ngay dữ liệu đã tải ở trang cha và `onChange` để nhận danh sách mới. Mọi request đi qua `lib/api/client.ts` để dùng access token của phiên đăng nhập.

Trong wizard, tạo recipe trước để có `recipeId`, rồi render component ở bước ảnh. Truyền `onPendingChange` để khóa nút rời bước khi ảnh còn đang tải; lỗi upload không khóa nút và có thể thử lại. API ảnh của issue #22 phải có trước khi thao tác thật.
