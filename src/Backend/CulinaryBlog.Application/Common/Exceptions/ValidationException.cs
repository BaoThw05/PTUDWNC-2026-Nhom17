using CulinaryBlog.Application.Common.Errors;

namespace CulinaryBlog.Application.Common.Exceptions;

/// <summary>
/// Lỗi validation hoặc vi phạm quy tắc nghiệp vụ, trả về HTTP 422 (S-10b).
/// </summary>
public sealed class ValidationException : AppException
{
    public ValidationException(IReadOnlyDictionary<string, string[]> errors)
        : base(ErrorCodes.ValidationError, "One or more validation errors occurred.")
    {
        Errors = errors;
    }

    public ValidationException(string code, string message)
        : base(code, message)
    {
        Errors = new Dictionary<string, string[]>();
    }

    public IReadOnlyDictionary<string, string[]> Errors { get; }
}
