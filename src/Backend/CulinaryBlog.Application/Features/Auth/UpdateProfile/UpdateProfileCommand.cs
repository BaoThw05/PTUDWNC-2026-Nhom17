using CulinaryBlog.Application.Features.Auth.Common;
using MediatR;

namespace CulinaryBlog.Application.Features.Auth.UpdateProfile;

/// <summary>Chỉ đổi được tên hiển thị; ảnh đại diện lấy từ Google (S-17).</summary>
public sealed record UpdateProfileCommand(string DisplayName) : IRequest<UserProfileResponse>;
