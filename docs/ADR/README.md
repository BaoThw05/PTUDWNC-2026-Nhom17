# Architecture Decision Records (ADR)

Mỗi quyết định kiến trúc quan trọng được ghi thành một file ngắn (tối đa khoảng một trang): bối cảnh, quyết định, hệ quả và phương án dự phòng. Phân tích đầy đủ các phương án nằm trong `docs/SRS_Culinary_Blog_v1.0.0_GiaiPhap.md`; ADR chỉ ghi lại điều nhóm đã chốt.

## Cách viết

1. Chép `0000-template.md` thành `ADR-NNN-<ten-ngan-tieng-Anh>.md` (số thứ tự tăng dần, không đánh lại) — khớp quy ước đã dùng ở `ADR-003`, `ADR-004`.
2. Điền nội dung, để trạng thái **Đề xuất** và mở PR. Người review đồng ý thì đổi sang **Đã chốt**.
3. Thêm một dòng vào bảng dưới và liên kết ADR vào cột "ADR" của Bảng Change Request trong `docs/SRS_v1.1.md` (nếu quyết định làm SRS thay đổi).
4. Quyết định thay đổi về sau: viết ADR mới, đổi ADR cũ sang **Bị thay thế bởi NNN**; không sửa nội dung ADR đã chốt.

> **Lưu ý quy ước (22/09):** thư mục từng là `docs/adr/` (chữ thường); đã đổi sang `docs/ADR/` (chữ hoa) để khớp với `ADR-003`/`ADR-004` do TV3 tạo trên `main` — hai cách viết hoa/thường khác nhau gộp thành một thư mục vật lý trên Windows nên không thể tồn tại song song. TV2 vẫn đang dùng `docs/adr/000N-<ten>.md` (chữ thường, không có tiền tố `ADR-`) trên nhánh riêng chưa merge — cần nhóm thống nhất lại một quy ước trước khi merge nhánh đó.

## Danh sách (mốc M0 — CN 20/09)

| ADR | Quyết định | Phụ trách | Trạng thái |
|---|---|---|---|
| [ADR-001](./ADR-001-kien-truc-xac-thuc.md) | S-05 · Kiến trúc xác thực Next.js ↔ .NET | TV1 | Đã chốt |
| [ADR-002](./ADR-002-refresh-token.md) | S-06 · Refresh token: xoay vòng, phát hiện dùng lại | TV1 | Đã chốt |
| [ADR-003](./ADR-003-object-storage.md) | S-09 · Storage S3 trung lập, Cloudflare Tunnel | TV3 | Đã chốt |
| [ADR-004](./ADR-004-recipe-image-handling.md) | S-12 · Upload và xử lý ảnh | TV3 | Đã chốt |
| — | S-03 · Xóa dữ liệu (lai, thùng rác 30 ngày) | TV2 | Chưa viết |
| — | S-04 · Kiểm soát đồng thời bằng `xmin` | TV2 | Chưa viết |
| — | S-10 · Hợp đồng API chung | TV2 | Chưa viết |
| — | S-11 · Vòng đời công thức, publish, slug | TV2 | Chưa viết |
| — | S-13 · Ranh giới Clean Architecture, MediatR 12.5.0 | TV2 | Chưa viết |
| — | S-07 · Chiến lược cache | TV4 | Chưa viết |
| — | S-08 · Tìm kiếm tiếng Việt không dấu | TV4 | Chưa viết |
| — | S-14 · Sitemap và background job | TV4 | Chưa viết |
| — | S-16 · NFR mức đồ án | TV4 | Chưa viết |
| — | S-18 · Phiên bản công nghệ | TV4 | Chưa viết |
