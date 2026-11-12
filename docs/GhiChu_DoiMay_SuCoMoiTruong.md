# Ghi chú: sự cố môi trường khi chuyển máy (12/11/2026)

> Người ghi: TV1 (Nguyễn Ngọc Tuấn). Máy gặp lỗi: bản clone cũ ở `C:\CTK47C\PTUDWeb\CulinaryBlog`, so với máy desktop đã chạy ổn.

## Nguyên nhân gốc

Git chỉ mang theo **mã nguồn**. Những thứ sau nằm ngoài git nên mỗi máy có trạng thái riêng: `.env`, `node_modules`, image/container/volume Docker, dữ liệu trong Postgres và MinIO, tiến trình đang chạy. Máy cũ được dựng từ bản tháng 9 rồi chỉ cập nhật code, nên các thứ này lệch với code mới.

## Các sự cố và cách xử lý

| # | Triệu chứng | Nguyên nhân | Cách xử lý |
|---|---|---|---|
| 1 | `docker compose` báo thiếu `HANGFIRE_PASSWORD`, `AUTH_SECRET`, `AUTH_URL`, `SEQ_ADMIN_PASSWORD`, các biến `*_PORT` | `.env` tạo từ tháng 9; `docker-compose.yml` tháng 10 đòi thêm biến mới. `.env` nằm trong `.gitignore` nên không tự cập nhật | So `.env` với `.env.example`, thêm biến còn thiếu (giữ giá trị cũ). `AUTH_SECRET` nên sinh ngẫu nhiên: `openssl rand -base64 32` |
| 2 | `quay.io/minio/minio@sha256:... 401 UNAUTHORIZED` | Từ 9/2026 MinIO đóng quyền tải ẩn danh trên quay.io (cả Docker Hub). Máy đã có image trong bộ nhớ đệm thì không bị; máy mới/xoá image thì bị | Tạo `docker-compose.override.yml` (local, không commit) trỏ `minio` và `minio-init` sang mirror `icr.io/fusion-open/minio/minio@sha256:14cea493d9a34af32f524e538b8346cf79f3321eff8e708c1e2960462bd8936e` (cùng digest ADR-003). **Cần quyết định chung của nhóm**, xem mục "Việc nhóm cần quyết" |
| 3 | `Module not found: Can't resolve 'axios'` | `node_modules` cũ, trước khi `axios` được thêm vào `package.json` | `npm install` trong `src/Frontend`. `mo-web.bat` (bản local) nay luôn chạy `npm install` |
| 4 | Web chạy nhưng mọi danh sách báo "Không thể tải"; API trả 500; log Postgres: `Role "culinary_admin" does not exist` | (a) `.env` cũ dùng tên DB/tài khoản/mật khẩu khác với `appsettings.Development.json`; volume Postgres giữ tài khoản cũ. (b) Tiến trình API cũ vẫn giữ cổng 5000 nên API mới không khởi động được, dù đã sửa cấu hình | Đặt `POSTGRES_*`, `REDIS_PASSWORD`, `MINIO_ROOT_*` trong `.env` đúng giá trị mặc định của `.env.example`; `docker compose down -v` để tạo lại volume; tắt tiến trình đang giữ cổng 5000/3000 trước khi chạy lại |
| 5 | Tab `localhost:8025` hiện MailHog thay vì Mailpit | Container `culinary_mailhog` từ compose cũ vẫn sống, chiếm cổng 8025/1025 nên Mailpit không lên | `docker rm -f culinary_mailhog` |
| 6 | Giao diện sáng, thiếu bảng điều khiển/ảnh danh mục so với desktop | Máy lỗi đang ở nhánh `2312793/tv1-tuan8` (lịch sử cũ, lệch hẳn `main`), desktop ở `2312793/tv1-mo-rong` | `git fetch`, tạo nhánh sao lưu, `git switch 2312793/tv1-mo-rong`, `npm install` |
| 7 | Có dữ liệu mẫu nhưng thiếu ảnh | Dữ liệu và ảnh thật nằm trong **volume Postgres và MinIO** của desktop, không nằm trong git. Seeder (`RecipeDataSeeder`) chỉ gắn ảnh mẫu cho 1/10 công thức | Chép dữ liệu từ desktop: `pg_dump -Fc` + `tar` thư mục `/data` của MinIO, nạp lại bằng `pg_restore` + giải nén |
| 8 | Sau khi nạp dữ liệu vẫn không thấy ảnh | (a) Quyền đọc công khai của bucket MinIO nằm trong `.minio.sys`, bị mất khi ghi đè `/data`. (b) Cache Redis (output cache) còn giữ danh sách cũ | `mc anonymous set download local/culinary-blog` (trong container `culinary_minio`) và `redis-cli flushall`. Sau đó xoá `src/Frontend/.next` nếu trang vẫn giữ bản cũ |

## Hướng dẫn xử lý từng bước (PowerShell, tại thư mục gốc dự án)

### A. Đồng bộ code và môi trường (lỗi 3, 6)

```powershell
git fetch origin
git status -sb                                  # xem đang ở nhánh nào, có commit chưa push không
git branch backup/<ten-nhanh-hien-tai>          # giữ lại commit cục bộ trước khi đổi nhánh
git stash                                       # cất thay đổi đang sửa dở (nếu có)
git switch <nhanh-can-dung>                     # ví dụ 2312793/tv1-mo-rong
cd src\Frontend; npm install; cd ..\..
```

### B. Sửa `.env` (lỗi 1, 4)

```powershell
Copy-Item .env .env.bak                         # sao lưu
# Mở .env.example, chép các biến .env còn thiếu sang .env (giữ giá trị cũ nếu đã có)
# Đặt lại cho khớp appsettings.Development.json:
#   POSTGRES_DB=CulinaryBlogDb  POSTGRES_USER=postgres  POSTGRES_PASSWORD=postgrespassword
#   REDIS_PASSWORD=change_me_dev_password
#   MINIO_ROOT_USER=minioadmin  MINIO_ROOT_PASSWORD=change_me_minio_password
# AUTH_SECRET: chuỗi ngẫu nhiên >= 32 ký tự (openssl rand -base64 32)
```

### C. Dọn container và tiến trình cũ (lỗi 4, 5)

```powershell
docker ps -a                                    # tìm container không còn trong docker-compose.yml (vd culinary_mailhog)
docker rm -f culinary_mailhog
netstat -ano | findstr ":5000 :3000"            # tìm PID đang giữ cổng
taskkill /pid <PID> /t /f
docker compose down -v                          # XOÁ volume local: chỉ dùng khi chấp nhận mất dữ liệu Postgres/MinIO máy này
```

### D. MinIO không kéo được image (lỗi 2)

Tạo `docker-compose.override.yml` ở thư mục gốc (đã có trong `.git/info/exclude`, không commit):

```yaml
services:
  minio:
    image: icr.io/fusion-open/minio/minio@sha256:14cea493d9a34af32f524e538b8346cf79f3321eff8e708c1e2960462bd8936e
  minio-init:
    image: icr.io/fusion-open/minio/minio@sha256:14cea493d9a34af32f524e538b8346cf79f3321eff8e708c1e2960462bd8936e
```

### E. Chép dữ liệu và ảnh từ máy khác (lỗi 7, 8)

Máy nguồn (đang có dữ liệu):

```powershell
docker exec culinary_postgres sh -c 'pg_dump -U "$POSTGRES_USER" -d "$POSTGRES_DB" -Fc -f /tmp/db.dump'
docker cp culinary_postgres:/tmp/db.dump .\db.dump
docker run --rm --volumes-from culinary_minio -v "${PWD}:/backup" alpine tar czf /backup/minio.tgz -C /data .
```

Máy đích (tắt API trước):

```powershell
docker cp .\db.dump culinary_postgres:/tmp/db.dump
docker exec culinary_postgres sh -c 'pg_restore -U "$POSTGRES_USER" -d "$POSTGRES_DB" --clean --if-exists --no-owner --no-acl /tmp/db.dump'
docker stop culinary_minio
docker run --rm --volumes-from culinary_minio -v "${PWD}:/backup" alpine sh -c "rm -rf /data/* /data/.minio.sys && tar xzf /backup/minio.tgz -C /data"
docker start culinary_minio
# Bắt buộc sau khi nạp: bật lại quyền đọc công khai và xoá cache
docker exec culinary_minio sh -c 'mc alias set local http://localhost:9000 "$MINIO_ROOT_USER" "$MINIO_ROOT_PASSWORD" && mc anonymous set download local/culinary-blog'
docker exec culinary_redis sh -c 'redis-cli --no-auth-warning -a "$REDIS_PASSWORD" flushall'
```

`db.dump` và `minio.tgz` chứa dữ liệu thật: **không commit**, xoá sau khi nạp.

### F. Chạy lại và kiểm tra

```powershell
docker compose up -d --wait postgres redis minio mailpit
docker compose up -d minio-init
# Cửa sổ 1: cd src\Backend\CulinaryBlog.API ; dotnet run
# Cửa sổ 2: cd src\Frontend ; npm run dev
curl http://localhost:5000/health/ready         # phải trả Healthy
curl "http://localhost:5000/api/v1/recipes?page=1&pageSize=2"   # phải trả danh sách, không phải 500
```

Nếu API vẫn trả 500: đọc log cửa sổ API và `docker logs --tail 30 culinary_postgres` (tìm `password authentication failed`).
Nếu trang web còn bản cũ: đóng cửa sổ Web, xoá `src\Frontend\.next`, chạy lại `npm run dev`, rồi Ctrl+F5.

## Lưu ý khi chẩn đoán

- Log của cửa sổ API là nơi hiện lỗi thật; chỉ nhìn giao diện web thì chỉ thấy "Không thể tải".
- `docker ps -a` và `netstat -ano | findstr :5000` là hai lệnh nhanh nhất để phát hiện container/tiến trình cũ chiếm cổng.
- Lỗi 1, 3, 4 đều cùng loại: **môi trường cục bộ không được đồng bộ sau khi pull code mới**.

## Checklist mỗi lần `git pull`, đổi nhánh hoặc chuyển máy

1. `git status -sb` — xác nhận đúng nhánh, không có commit chưa push bị bỏ quên.
2. So `.env` với `.env.example`; thêm biến mới nếu có.
3. `npm install` trong `src/Frontend`.
4. `docker ps -a` — gỡ container cũ không còn trong `docker-compose.yml`; nếu compose đổi cấu hình dịch vụ thì `docker compose down` trước khi `up`.
5. Tắt tiến trình API/Web cũ (cổng 5000, 3000) rồi mới chạy lại.
6. Muốn có đúng dữ liệu của máy khác: xuất/nạp cả **Postgres và MinIO**, rồi đặt lại quyền đọc công khai bucket và xoá cache Redis. Nhánh `integration/merge-all-2026-11-12` có commit `e81fe59` (TV3) "script sao lưu và khôi phục dữ liệu demo", nên ưu tiên dùng script đó khi đã gộp vào `main`.

## Việc nhóm cần quyết

- **Nguồn image MinIO** (ảnh hưởng mọi người clone mới và có thể cả CI): `docker-compose.yml` hiện ghim `quay.io/minio/minio@sha256:...` và không còn tải được. Cần PR riêng của TV3 (hạ tầng) để đổi nguồn image và cập nhật ADR-003.
