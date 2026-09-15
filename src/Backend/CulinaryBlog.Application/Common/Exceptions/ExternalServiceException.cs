namespace CulinaryBlog.Application.Common.Exceptions;

/// <summary>Một dịch vụ bên ngoài (VD: Google) không khả dụng hoặc trả lỗi -> HTTP 502 Bad Gateway.</summary>
public class ExternalServiceException : Exception
{
    public ExternalServiceException(string message, Exception? innerException = null)
        : base(message, innerException)
    {
    }
}
