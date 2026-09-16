# RecipeImages — Infrastructure

- **Phụ trách:** TV3 — Lương Đức Sang
- **FR:** FR-RCP-008, FR-FILE, FR-JOB-002
- **Nhóm route:** `/api/v1/recipes/{recipeId:guid}/images`

## Việc cần làm ở tầng này

`S3FileStorage`, `LocalFileStorage`, Hangfire, job resize ảnh (3.05, 3.07, 3.16).

Xem kế hoạch chi tiết trong `docs/KeHoach/` và quy tắc trong `README.md` gốc.
