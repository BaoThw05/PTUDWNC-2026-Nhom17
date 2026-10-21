# Sao lưu và khôi phục bản demo

Hai script PowerShell trong `scripts/` sao lưu PostgreSQL bằng `pg_dump -Fc` và
nén toàn bộ volume MinIO thành `minio.tar.gz`. Bộ backup gồm ba file:
`postgres.dump`, `minio.tar.gz` và `manifest.json` chứa SHA-256. Thư mục
`backups/` được Git bỏ qua; lưu bộ backup ở nơi khác ngoài máy chạy demo.

## Sao lưu

Chạy tại thư mục gốc repository, sau khi tạo `.env` và khởi động stack:

```powershell
docker compose up -d
./scripts/backup.ps1
```

Có thể chọn nơi lưu và project Compose:

```powershell
./scripts/backup.ps1 -OutputDirectory 'D:\CulinaryBlogBackup\2026-10-21' -ProjectName culinaryblog
```

Script tạm dừng web, API và MinIO để database và ảnh thuộc cùng một thời điểm,
sau đó tự bật lại những service vốn đang chạy. PostgreSQL tiếp tục chạy để
`pg_dump` thực hiện bản sao lưu nhất quán. Không tắt máy hoặc xóa volume trong
lúc script đang chạy. Chỉ dùng bản backup có đủ ba file và thông báo thành công.

## Khôi phục

Trên máy nhận, clone đúng phiên bản repository, cài Docker, tạo `.env` từ
`.env.example` và điền cấu hình. Giữ `MINIO_BUCKET` giống máy nguồn và dùng
cùng phiên bản image PostgreSQL và MinIO trong `docker-compose.yml`; không cần
sao chép mật khẩu từ máy cũ. Chuyển cả
ba file backup vào một thư mục trên máy nhận rồi chạy:

```powershell
./scripts/restore.ps1 -BackupDirectory 'D:\CulinaryBlogBackup\2026-10-21' -Force
docker compose up -d
```

Script kiểm tra SHA-256 và đọc thử hai archive trước khi thay đổi dữ liệu. Nó
khôi phục database bằng `pg_restore` và giải nén ảnh vào volume MinIO. Mặc định
volume ảnh phải trống. Nếu khôi phục đè lên stack đã có ảnh, trước tiên giữ một
bản backup riêng của stack đó, rồi chạy thêm `-ReplaceExisting -Force`:

```powershell
./scripts/restore.ps1 -BackupDirectory 'D:\CulinaryBlogBackup\2026-10-21' -ReplaceExisting -Force
docker compose up -d
```

Lệnh khôi phục đè sẽ xóa nội dung volume ảnh hiện có và thay thế dữ liệu trong
database của project được chọn. Script kiểm tra nhãn volume Compose trước khi
xóa. Nếu lỗi xảy ra sau khi dịch vụ đã dừng, xử lý lỗi rồi chạy lại lệnh khôi
phục; sau đó mới bật ứng dụng. Không commit hoặc chia sẻ công khai bộ backup vì
nó chứa dữ liệu người dùng và ảnh.

## Kiểm tra sau khôi phục

```powershell
docker compose ps
docker compose exec postgres psql -U postgres -d CulinaryBlogDb -c '\dt'
```

Mở site, kiểm tra vài công thức, ảnh gốc và ảnh biến thể qua `/media/`. So sánh
số danh mục, công thức và ảnh với máy nguồn. Nếu dùng tên database/user khác
trong `.env`, thay tham số `psql` tương ứng.
