using CulinaryBlog.Application.Common.Errors;

namespace CulinaryBlog.Application.Common.Exceptions;

public sealed class UnauthorizedException(string message = "Bạn cần đăng nhập để thực hiện thao tác này.", string code = ErrorCodes.Unauthorized)
    : AppException(code, message);
