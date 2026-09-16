# Recipes — Infrastructure

- **Phụ trách:** TV2 — Bùi Ngọc Toàn
- **FR:** FR-RCP-002 → 007, 009, 010
- **Nhóm route:** `/api/v1/recipes`, `/api/v1/me`

## Việc cần làm ở tầng này

Cấu hình EF, partial unique index `Slug`, migration `Recipes_Init`; `AuditInterceptor`, `IUnitOfWork`, `ISoftDeletable` (2.04, 2.06).

Xem kế hoạch chi tiết trong `docs/KeHoach/` và quy tắc trong `README.md` gốc.
