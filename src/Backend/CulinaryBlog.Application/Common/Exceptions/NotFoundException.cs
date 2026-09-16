using CulinaryBlog.Application.Common.Errors;

namespace CulinaryBlog.Application.Common.Exceptions;

public sealed class NotFoundException(string message, string code = ErrorCodes.NotFound)
    : AppException(code, message);
