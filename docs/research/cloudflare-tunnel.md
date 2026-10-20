# Nghiên cứu Cloudflare Tunnel cho bản demo

**Việc:** 3.06 · **Người thực hiện:** Lương Đức Sang (TV3) · **Ngày:** 26/09/2026

## Kết luận

Cloudflare Tunnel phù hợp để đưa một bản demo đang chạy trên máy nhóm ra
Internet mà không cần VPS, IP tĩnh hoặc mở cổng inbound trên router. Tunnel
không thay thế MinIO/S3: file vẫn nằm trong object storage, còn `cloudflared`
chỉ chuyển lưu lượng HTTPS từ Cloudflare về Nginx của ứng dụng.

Khuyến nghị của nhóm:

- Dùng **Quick Tunnel** khi cần gửi link ngắn hạn cho giảng viên hoặc thành viên
  kiểm tra trong ngày
- Dùng **Named Tunnel** khi cần một URL ổn định cho buổi demo chính thức,
  callback OAuth hoặc webhook
- Chỉ bổ sung service `cloudflared` ở Compose profile `demo` trong việc 3.18;
  không chạy Tunnel thường trực trong môi trường development

## Quick Tunnel và Named Tunnel

| Tiêu chí | Quick Tunnel | Named Tunnel |
|---|---|---|
| URL | Ngẫu nhiên dạng `*.trycloudflare.com`, đổi sau mỗi lần chạy | Hostname do nhóm chọn, ổn định |
| Tài khoản/domain | Không cần tài khoản, domain hoặc DNS | Cần tài khoản Cloudflare và zone/domain do nhóm quản lý trên Cloudflare |
| Cách khởi động | `cloudflared tunnel --url http://localhost:8080` | Tạo tunnel trong dashboard, cấu hình public hostname rồi chạy bằng token |
| Mục đích | Test/development, demo ngắn hạn | Demo ổn định và các tích hợp cần URL cố định |
| Giới hạn | Không có SLA, tối đa 200 request đang xử lý, không hỗ trợ SSE | Không có các giới hạn riêng của Quick Tunnel; vẫn phải quản lý token và Access |

Cloudflare xác định Quick Tunnel chỉ dành cho testing/development. URL ngẫu
nhiên là ưu điểm khi không có domain, nhưng không thể dùng làm link nộp bài hay
callback cố định. Quick Tunnel cũng không chạy được nếu thư mục `.cloudflared`
có `config.yaml`; cần dùng lệnh trực tiếp hoặc tạm đổi tên file cấu hình.
[Tài liệu Quick Tunnel của Cloudflare](https://developers.cloudflare.com/cloudflare-one/networks/connectors/cloudflare-tunnel/do-more-with-tunnels/trycloudflare/)

Named Tunnel (tên mới trong dashboard thường là remotely-managed tunnel) phù
hợp cho demo chính thức. Nhóm tạo tunnel, đặt public hostname ví dụ
`demo.example.com`, trỏ service về `http://nginx:80`, sau đó chạy connector với
tunnel token. Để dùng hostname riêng, domain phải được thêm vào Cloudflare và
DNS của domain phải do Cloudflare quản lý.
[Hướng dẫn tạo tunnel chính thức](https://developers.cloudflare.com/tunnel/get-started/)

## Chạy `cloudflared` trong Docker Compose

Khi làm việc 3.18, service nên nằm sau profile `demo`, đi tới Nginx trong cùng
network Docker. Không publish thêm port cho `cloudflared`; connector tự tạo kết
nối outbound tới Cloudflare.

```yaml
  cloudflared:
    image: cloudflare/cloudflared:latest
    profiles: ["demo"]
    restart: unless-stopped
    depends_on:
      nginx:
        condition: service_started
    command: tunnel --no-autoupdate run --token ${CLOUDFLARE_TUNNEL_TOKEN:?copy .env.example to .env first}
```

Chạy Named Tunnel:

```bash
docker compose --profile demo up -d cloudflared
docker compose logs -f cloudflared
```

Trong dashboard Cloudflare, public hostname phải trỏ về service nội bộ
`http://nginx:80`, không trỏ thẳng API, PostgreSQL, Redis, MinIO console,
Seq, Mailpit hoặc Hangfire. Trước khi commit Compose thật, image phải được ghim
version hoặc digest theo quy ước hạ tầng của nhóm; `latest` ở trên chỉ là ví dụ
trích từ tài liệu Cloudflare.

Với Quick Tunnel, chạy một process tạm thời từ máy host hoặc một container
không có `config.yaml`:

```bash
cloudflared tunnel --url http://localhost
```

Lệnh in URL `trycloudflare.com`; URL này mất hiệu lực khi process dừng hoặc
khởi động lại. Không đưa URL Quick Tunnel vào tài liệu cố định.

## Bảo mật và lưu ý demo

- `CLOUDFLARE_TUNNEL_TOKEN` là secret. Ai có token có thể chạy connector cho
  tunnel, vì vậy chỉ để trong `.env`/secret store, không commit, không chụp màn
  hình và không in log
- Nếu token nghi bị lộ, rotate token trên dashboard rồi cập nhật toàn bộ
  connector; token cũ không tạo kết nối mới được
- Chỉ expose Nginx public site. Tắt hoặc bảo vệ các route quản trị như
  `/hangfire`; không expose dashboard MinIO, Mailpit, Seq, database hay Redis
- Demo phải dùng dữ liệu giả, không dùng thông tin đăng nhập hoặc dữ liệu cá
  nhân thật
- Kiểm tra URL công khai trên mạng khác trước buổi demo; kiểm tra upload ảnh và
  URL ảnh qua Nginx khi việc 3.17 đã hoàn tất
- Nếu Tunnel hoặc DNS lỗi, phương án dự phòng là demo tại `http://localhost`

Tunnel token có thể được lấy và rotate trong Cloudflare Dashboard. Cloudflare
khuyến cáo coi token là bí mật và nêu rõ token cũ không dùng được cho kết nối
mới sau khi rotate.
[Tài liệu tunnel token](https://developers.cloudflare.com/tunnel/reference/tunnel-tokens/)

## Checklist trước khi bật profile `demo`

1. Docker Compose, Nginx và `/health` đã xanh tại local
2. Public hostname trong Cloudflare trỏ về `http://nginx:80`
3. Token có trong `.env` local, không có trong Git history hoặc log
4. Route quản trị đã tắt hoặc có xác thực
5. Kiểm tra site từ 4G/mạng ngoài; nếu lỗi, chuyển sang kịch bản localhost

