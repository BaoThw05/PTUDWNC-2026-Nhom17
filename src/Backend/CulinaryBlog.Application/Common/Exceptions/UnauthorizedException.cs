using CulinaryBlog.Application.Common.Errors;

namespace CulinaryBlog.Application.Common.Exceptions;

/// <summary>Chưa xác thực hoặc thông tin xác thực không hợp lệ, trả về HTTP 401.</summary>
public sealed class UnauthorizedException(string message, string code = ErrorCodes.Unauthorized)
    : AppException(code, message);
