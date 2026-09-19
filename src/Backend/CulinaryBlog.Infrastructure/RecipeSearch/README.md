# RecipeSearch — Infrastructure

- **Phụ trách:** TV4 — Trần Lê Bảo Thư
- **FR:** FR-RCP-001, FR-SRCH-001 → 004
- **Nhóm route:** `/api/v1/recipes`

## Việc cần làm ở tầng này

`pg_trgm` + cột không dấu, Output Cache / `ICacheInvalidator` (4.06, 4.08, 4.14).

Xem kế hoạch chi tiết trong `docs/KeHoach/` và quy tắc trong `README.md` gốc.
