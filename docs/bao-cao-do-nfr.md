# Báo cáo đo NFR – thành viên 4

Ngày đo: 20/11/2026
Phạm vi: công việc 4.18–4.22; demo local trên Docker Compose tại `http://localhost`.

## Kết quả

| Phép đo | Cấu hình | Kết quả | Đánh giá |
|---|---|---|---|
| k6 API | 50 VU, 5 phút; danh sách, chi tiết, tìm kiếm; Linux Docker x86_64, 16 CPU, 7,6 GiB RAM | 44.701 request; 148,63 req/s; p95 5,30 ms; lỗi 0%; 44.701/44.701 checks | Đạt ngưỡng script p95 < 500 ms, lỗi < 1% |
| Lighthouse mobile, trang công thức | Lighthouse 13.6.0, Microsoft Edge 157, URL local | Performance 92; SEO 100; Accessibility 100; Best Practices 100; LCP mô phỏng 3.350 ms; LCP quan sát 294 ms; FCP mô phỏng 925 ms; TBT 40 ms; CLS 0 | Chưa đạt LCP mô phỏng ≤ 2.500 ms; CLS đạt |
| Rich Results Test, chế độ Code | HTML/JSON-LD của 3 công thức được capture từ demo local | Google nhận nội dung code; sau khi gửi hiển thị “Something went wrong — Log in and try again” | Chưa xác minh được kết quả Google vì yêu cầu đăng nhập. Không dùng URL public; ảnh `localhost` cũng không thể được Google truy cập |
| Smoke UI/SEO | Trang chủ, danh mục món chính và 3 trang công thức | Tất cả HTTP 200; canonical, Open Graph, Twitter Card có mặt; ảnh 16/16 tải được; JSON-LD Recipe có nguyên liệu, các bước, tác giả, thời gian, ảnh | Đạt kiểm tra local |

Lighthouse mobile có mô phỏng mạng/CPU chậm nên LCP mô phỏng khác LCP quan sát của lần chạy local. Báo cáo giữ cả hai số liệu để không che ngưỡng LCP chưa đạt.

## Tái lập

```sh
k6 run tests/performance/recipes.js
```

Mặc định script dùng `http://localhost/api/v1`; có thể đặt `API_BASE_URL` để đo API local khác. Script tạo `runId` riêng tránh cache kết quả giữa các lần đo. Dùng `tests/performance/capture-local-demo.mjs` để kiểm tra trang và lưu HTML/screenshot. Lighthouse chạy trên URL local:

```sh
npx lighthouse http://localhost/recipes/sample-39-pho-bo --preset=desktop --output=json --output=html
```

Để xác thực Rich Results qua URL, cần một demo public có thể truy cập và ảnh public; ở yêu cầu hiện tại chỉ dùng local. Kết quả gửi code hiện bị chặn bởi phiên đăng nhập Google nên không ghi là đã đạt.

## Bằng chứng

- k6: `docs/evidence/tv4/k6-run.log`, `k6-summary.json`.
- Lighthouse trước/sau: `lighthouse-before.json`, `lighthouse-final-preload.report.json`.
- Smoke: `local-smoke.json`, `home.png`, `category.png`, `recipe-pho-bo.png`.
- HTML dùng trong Google Rich Results Test: `recipe-pho-bo-rich-results.html`, `recipe-chuoi-nep-rich-results.html`, `recipe-dua-tac-rich-results.html`; ảnh màn hình: `rich-results-code-input.png`.
- Tham khảo: [Lighthouse 13.6.0 release](https://github.com/GoogleChrome/lighthouse/releases), [Google Rich Results Test](https://search.google.com/test/rich-results).

## Tối ưu trong lần hoàn thiện

Đã server-render ảnh đầu tiên của recipe, đặt preload ưu tiên cao cho ảnh LCP, chỉ tải gallery tương tác khi có nhiều ảnh, cache đọc public recipe/image 5 phút, và mở ảnh công thức Published cho guest. So với lần Lighthouse đầu, Performance tăng từ 88 lên 92, LCP mô phỏng từ 3.800 xuống 3.350 ms, CLS từ 0,02 xuống 0. Ngưỡng LCP vẫn cần tối ưu thêm.
