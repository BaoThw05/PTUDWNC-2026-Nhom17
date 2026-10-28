# RecipeImages — Infrastructure

- **Phụ trách:** TV3 — Lương Đức Sang
- **FR:** FR-RCP-008, FR-FILE, FR-JOB-002
- **Nhóm route:** `/api/v1/recipes/{recipeId:guid}/images`

## Job dọn file mồ côi (3.19)

Hangfire chạy `recipe-image-orphan-cleanup` lúc 03:00 UTC mỗi ngày. Job chỉ xóa prefix
`recipes/{recipeId}/{imageId}/` khi không còn bản ghi ảnh trong DB, công thức không
nằm trong thùng rác và toàn bộ file trong prefix đã cũ hơn một giờ. Các key không
đúng cấu trúc này được bỏ qua. Danh sách file được đọc qua `IFileStorage` nên hoạt
động với cả local storage và S3/MinIO.

Xem kế hoạch chi tiết trong `docs/KeHoach/` và quy tắc trong `README.md` gốc.
