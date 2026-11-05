using CulinaryBlog.Application.Features.Auth.Common;
using MediatR;

namespace CulinaryBlog.Application.Features.Auth.ChangeEmail;

/// <summary>Bước 2: nhập mã OTP đã gửi tới email mới.</summary>
public sealed record ConfirmEmailChangeCommand(string NewEmail, string Code) : IRequest<UserProfileResponse>;
