# ADR-003: Lựa chọn Object Storage cho môi trường Development

- **Ngày**: 19/09/2026
- **Trạng thái**: Đã chấp nhận (Accepted)
- **Người thực hiện**: Lương Đức Sang (TV3)
- **Liên quan lỗi**: F-01 (SRS_Culinary_Blog_v1.0.0_DanhSachLoi.md)

## Bối cảnh

SRS gốc chỉ định dùng **MinIO** (image `minio/minio` trên Docker Hub) làm Object
Storage tương thích S3 cho môi trường phát triển. Tại thời điểm rà soát
(tháng 09/2026), MinIO đã ngừng phát hành bản Community Edition dạng image
Docker Hub công khai, khiến `docker pull minio/minio:latest` có nguy cơ thất bại.

## Đã thử nghiệm (19/09/2026)

| Lệnh | Kết quả |
|---|---|
| `docker pull minio/minio:latest` | ❌ `pull access denied for minio/minio, repository does not exist` |
| `docker pull minio/minio:RELEASE.2025-04-08T15-41-24Z` | ❌ Thất bại (không tồn tại) |
| `docker pull quay.io/minio/minio:latest` | ✅ **Thành công** |

Digest xác nhận kéo được:
```
quay.io/minio/minio@sha256:14cea493d9a34af32f524e538b8346cf79f3321eff8e708c1e2960462bd8936e
```

## Quyết định

1. **Dùng `quay.io/minio/minio`** thay cho `minio/minio` (Docker Hub) làm image
   MinIO cho môi trường Development, ghi trong `docker-compose.yml`.
2. **Ghim theo digest cụ thể** (`@sha256:...`) thay vì tag `:latest`, để tránh
   rủi ro image bị thay đổi/gỡ bỏ ngầm trong tương lai (đúng tinh thần khuyến
   nghị D-15 — ghim version mọi image hạ tầng).
3. Tầng code (`IFileStorageService`) vẫn được thiết kế **trung lập nhà cung
   cấp**, dùng `AWSSDK.S3` với `ServiceURL`/`ForcePathStyle` cấu hình được qua
   `appsettings.json` — nếu quay.io cũng ngừng phát hành trong tương lai, chỉ
   cần đổi image + cấu hình, **không cần sửa code**.
4. Có phương án dự phòng `LocalFileStorage` (lưu file ngay trên đĩa máy chủ,
   implement cùng interface `IFileStorageService`) — không cần dùng đến ở lần
   kiểm tra này vì quay.io đã kéo thành công, nhưng giữ lại trong code như một
   lựa chọn cấu hình cho các máy dev khác có thể gặp lại vấn đề tương tự.

## Hệ quả

- `docker-compose.yml` đã cập nhật dùng digest trên.
- Repo/README cần lưu ý: đây là quyết định **khác với SRS gốc** (ghi thẳng
  "MinIO" không nêu registry) — cần báo giảng viên theo khuyến nghị S-09.
- Nếu máy dev khác trong nhóm chạy `docker compose up -d` mà digest trên
  không còn tồn tại (do quay.io gỡ sau này), lặp lại quy trình thử nghiệm ở
  mục trên và cập nhật digest mới vào file này + `docker-compose.yml`.
