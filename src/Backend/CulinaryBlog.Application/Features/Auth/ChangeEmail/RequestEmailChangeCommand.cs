using MediatR;

namespace CulinaryBlog.Application.Features.Auth.ChangeEmail;

/// <summary>Bước 1: gửi mã OTP tới email mới. Cần mật khẩu hiện tại để phiên bị lộ không đổi được email rồi chiếm tài khoản.</summary>
public sealed record RequestEmailChangeCommand(string NewEmail, string CurrentPassword) : IRequest;
