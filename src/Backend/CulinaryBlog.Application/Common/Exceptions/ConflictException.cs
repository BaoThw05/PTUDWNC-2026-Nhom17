using CulinaryBlog.Application.Common.Errors;

namespace CulinaryBlog.Application.Common.Exceptions;

public sealed class ConflictException(string message, string code = ErrorCodes.Conflict)
    : AppException(code, message);
