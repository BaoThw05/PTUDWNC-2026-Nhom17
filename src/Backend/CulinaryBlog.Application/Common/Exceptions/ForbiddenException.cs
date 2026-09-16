using CulinaryBlog.Application.Common.Errors;

namespace CulinaryBlog.Application.Common.Exceptions;

public sealed class ForbiddenException(string message, string code = ErrorCodes.Forbidden)
    : AppException(code, message);
