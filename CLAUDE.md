# CLAUDE.md — Culinary Blog (Nhóm 17, PTUDW Nâng cao)

Hướng dẫn cho Claude Code khi làm việc trong repo này.

## Bối cảnh cố định

- Nhóm 4 người: **TV1** Nguyễn Ngọc Tuấn (Auth & Profile) · **TV2** Bùi Ngọc Toàn (Recipe Core) · **TV3** Lương Đức Sang (Category/File/Image) · **TV4** Trần Lê Bảo Thư (Danh sách/Tìm kiếm/SEO/Observability).
- Stack: .NET 10 Minimal APIs + Clean Architecture/CQRS (MediatR 12.5.0) · Next.js 16 App Router · PostgreSQL 16 · Redis 7.
- Máy dev: Windows + PowerShell. Kiểm tra Docker Desktop/WSL2 đang chạy trước khi chạy container.
- Tài liệu nguồn: `docs/` — SRS v1.0.0 (.md), `SRS_..._GiaiPhap.md` (các quyết định S-01 → S-18, **thắng SRS khi mâu thuẫn**), `SRS_..._DanhSachLoi.md`, kế hoạch tổng và kế hoạch TV1–TV4 trong `docs/KeHoach/`.
- Remote: `https://github.com/BaoThw05/PTUDWNC-2026-Nhom17` — nhánh chính là **`main`**.

## Quy tắc khi làm việc

- Cách chạy, cổng dịch vụ, cách thêm module/endpoint/trang: `README.md` mục 2–5. Cổng chung 3000/5000/5432/6379; máy dùng cổng riêng phải ghi vào README mục 3.1.
- Quy tắc nhóm (nhánh, commit, migration, hợp đồng API, bảo mật, Definition of Done): `README.md` mục 6.
- Clean code backend/frontend: `README.md` mục 7.
- Chỉ sửa thư mục của người đang làm việc theo `docs/OWNERSHIP.md`; file dùng chung phải báo nhóm và làm PR riêng. Cần sửa phần của người khác → dừng và hỏi.
- Mã lỗi mới ghi vào `docs/api/error-codes.md` trong cùng PR.
- Frontend: đọc `src/Frontend/AGENTS.md` và `node_modules/next/dist/docs/` trước khi code (Next 16 có breaking changes).
- Không push, mở PR hay merge khi chưa được người dùng cho phép.
- Trước khi commit: `dotnet build` (0 warning), `dotnet test`, `dotnet format --verify-no-changes`, `npm run lint`, `npm run build`.
