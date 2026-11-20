# Báo cáo đo NFR base

Ngày lập: 20/11/2026

Phạm vi: TV4, NFR-PERF-001/002/005 và NFR-SEO-001.

## Trạng thái

Chưa có số đo nghiệm thu. Trong lần kiểm tra này, URL demo local `http://localhost/api/v1/recipes?page=1&pageSize=1` trả HTTP 502 qua Nginx, nên không có cơ sở lấy số liệu k6 trên ứng dụng. Lighthouse và Google Rich Results Test cần một frontend/public URL truy cập được; môi trường hiện chưa cung cấp URL đó. Không dùng số liệu giả hoặc kết quả từ một stack khác để thay thế.

## Kịch bản và tiêu chí

| Phép đo | Kịch bản | Tiêu chí base | Kết quả |
|---|---|---|---|
| k6 API | 50 VU, 5 phút; danh sách, chi tiết, tìm kiếm; script `tests/performance/recipes.js` | p95 < 500 ms; lỗi < 1% | Chưa đo: demo trả 502 |
| Lighthouse mobile | Trang chi tiết recipe trên public/demo URL | LCP ≤ 2,5 giây; CLS ≤ 0,1 | Chưa đo: chưa có frontend URL phục vụ được |
| Rich Results | 3 recipe Published có ảnh trên public URL | Không có lỗi bắt buộc | Chưa đo: Google không truy cập được localhost |

## Tái lập phép đo

Khi API demo hoạt động tại `/api/v1`, chạy:

```sh
k6 run tests/performance/recipes.js
```

Hoặc chỉ định origin khác:

```sh
API_BASE_URL=https://demo.example.com/api/v1 k6 run tests/performance/recipes.js
```

Sau đó chạy Lighthouse mobile trên `/recipes/{slug}` và kiểm tra 3 URL công thức bằng Google Rich Results Test. Ghi URL, ngày, cấu hình máy, p95/throughput/error rate, LCP/CLS và ảnh kết quả vào bảng trên trước khi chốt 4.18/4.19.

## Tối ưu

Đã có các index truy vấn recipe/search và Output Cache theo policy trong API. Chưa thể ghi kết quả trước/sau vì phép đo tải chưa chạy được; không kết luận NFR đã đạt chỉ từ build hoặc integration test.
