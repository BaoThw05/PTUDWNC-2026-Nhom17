# RecipeImages — API

- **Phụ trách:** TV3 — Lương Đức Sang
- **FR:** FR-RCP-008, FR-FILE, FR-JOB-002
- **Nhóm route:** `/api/v1/recipes/{recipeId:guid}/images`

## API

- `GET /api/v1/recipes/{recipeId}/images`: trả danh sách ảnh theo `orderIndex`, dùng cho wizard và trang sửa.
- `POST /api/v1/recipes/{recipeId}/images`: multipart `file`, `altText?`; JPEG/PNG/WebP tối đa 5 MB. Trả 201 với `imageId`, `originalUrl`, `mediumUrl`, `thumbnailUrl`, `altText`, `isPrimary`, `orderIndex`.
- `PATCH /api/v1/recipes/{recipeId}/images/{imageId}`: JSON với các trường tùy chọn `altText`, `isPrimary`, `orderIndex`. Gửi `altText: null` để xóa mô tả. Trả 200 với dữ liệu ảnh mới.
- `DELETE /api/v1/recipes/{recipeId}/images/{imageId}`: xóa bản ghi, chọn ảnh chính kế tiếp, xếp job xóa file theo prefix. Trả 204.

Cả bốn endpoint yêu cầu vai trò Author/Admin và chỉ tác giả của công thức hoặc Admin được thao tác. Ảnh đầu tiên tự thành ảnh chính. Sau khi upload, worker tạo `thumb.webp` 300×300 (crop giữa) và `medium.webp` rộng tối đa 800px; ảnh không decode được được đánh dấu lỗi và không retry.
