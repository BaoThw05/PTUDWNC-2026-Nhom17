# Observability — API

- **Phụ trách:** TV4 — Trần Lê Bảo Thư
- **FR:** FR-OBS-001, 002
- **Nhóm route:** — (endpoint vận hành `/health*`)

## Việc cần làm ở tầng này

`CorrelationIdMiddleware`, `/health/live`, `/health/ready` (4.05, 4.06). `LoggingBehavior` đặt ở `Application/Common/Behaviors/`.

Xem kế hoạch chi tiết trong `docs/KeHoach/` và quy tắc trong `README.md` gốc.
