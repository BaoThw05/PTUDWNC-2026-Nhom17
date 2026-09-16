namespace CulinaryBlog.Application.Common.Exceptions;

/// <summary>
/// Gốc của mọi lỗi nghiệp vụ; <see cref="Code"/> được trả về ở trường mở rộng <c>code</c> của Problem Details.
/// </summary>
public abstract class AppException(string code, string message) : Exception(message)
{
    public string Code { get; } = code;
}
