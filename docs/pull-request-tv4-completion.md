## Mã công việc

- [x] Mã công việc: 4.18, 4.19, 4.20, 4.21, 4.22
- [x] Mã yêu cầu SRS: NFR-PERF-001, NFR-PERF-002, NFR-PERF-005, NFR-SEO-001, NFR-SEO-002, FR-RCP-001, NFR-REL-003

## Nội dung

- Thay seed ngẫu nhiên bằng catalog mẫu xác định gồm 50 công thức có nguyên liệu, định lượng, các bước, ảnh; cập nhật 10 danh mục tham khảo từ Điện máy Xanh. Startup thay thế an toàn các recipe có slug `sample-*`, không xóa nội dung người dùng khác.
- Cho phép guest xem ảnh của công thức Published và giữ Draft không công khai; cache GET recipe/image public trong 5 phút.
- Hoàn thiện ảnh LCP render phía server, preload ảnh chính, gallery tương tác chỉ tải khi recipe có nhiều ảnh; bổ sung ảnh vào JSON-LD Recipe.
- Thêm capture smoke local, kiểm tra mã JSON-LD Rich Results, k6 scenario và lưu bằng chứng đo. Google Rich Results Test nhận code nhưng trả yêu cầu đăng nhập; báo cáo ghi đúng trạng thái chưa xác minh.
- Đối chiếu/cập nhật SRS v1.1 cho health/readiness, logging, layering và pipeline; tạo ghi chú handoff cho TV1.
- Bản báo cáo NFR: `docs/bao-cao-do-nfr.md`.

## Kiểm tra

- [x] `dotnet build` (Release, thực hiện trong `dotnet test`)
- [x] `dotnet test` (Release): Unit 209/209, Architecture 7/7, Integration 121/121.
- [x] `dotnet format --verify-no-changes`
- [x] `npm run lint`
- [x] `npm run build` (Next.js production build)
- [x] k6 local: 50 VU / 5 phút, p95 5,30 ms, lỗi 0%, 148,63 req/s.
- [x] Lighthouse mobile local: Performance 92, SEO/Accessibility/Best Practices 100; CLS 0. LCP mô phỏng 3,35 giây, chưa đạt ≤2,5 giây.
- [x] Smoke local: trang chủ, danh mục, 3 recipe HTTP 200; metadata và ảnh 16/16 tải được.
- [ ] Rich Results: HTML đã nạp vào chế độ Code; Google yêu cầu đăng nhập để chạy. Cần demo URL public nếu muốn xác minh URL và ảnh.

## Review

- [x] Không chứa secret hoặc dữ liệu nhạy cảm
- [x] Đã cập nhật tài liệu/bảng mã lỗi nếu cần
- [ ] Đã có reviewer khác nhóm tác giả

## Môi trường và bằng chứng

- Chỉ đo trên demo local; không publish demo.
- Bằng chứng: `docs/evidence/tv4/`.
- Handoff TV1: `docs/handoff/TV4-SRS-v1.1.md`.
