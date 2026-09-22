# Auth — Infrastructure

- **Phụ trách:** TV1 — Nguyễn Ngọc Tuấn
- **FR:** FR-AUTH-001 → 007, FR-JOB-001

## Đã có

- `ApplicationUser : IdentityUser<Guid>` (`FullName`, `AvatarUrl`, `IsActive`, `CreatedAt`; `UserName` kế thừa từ Identity, người dùng tự chọn khi đăng ký) — nằm ở đây để Domain không phụ thuộc Identity.
- Bảng `Users`, `Roles`, `UserRoles`, `UserLogins`… (đổi tên từ `AspNet*`) và `RefreshTokens` (chỉ lưu SHA-256, có `FamilyId`, `RevokedReason`); migration `Auth_Init`.
- `IdentityUserAccountService` (mật khẩu, lockout 5 lần/15 phút, liên kết Google), `RefreshTokenRepository`, `JwtAccessTokenIssuer` (HS256), `GoogleIdTokenValidator`.
- `AuthDataSeeder`: tạo role Admin/Author và tài khoản mẫu khi bật `Database:SeedOnStartup`.

Module khác cần khóa ngoại tới người dùng thì dùng `Guid` và cấu hình `HasOne<ApplicationUser>().WithMany().HasForeignKey(...)`.

## Còn lại

- `WelcomeEmailJob` + gửi mail qua Mailpit (1.15) — chờ `IBackgroundJobService` của TV3.
- Job dọn refresh token hết hạn quá 30 ngày (1.20) — chờ Hangfire của TV3.
