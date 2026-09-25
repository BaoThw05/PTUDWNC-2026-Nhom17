namespace CulinaryBlog.Application.Common.Exceptions;

/// <summary>Dịch vụ bên ngoài (Google, storage…) lỗi hoặc không khả dụng, trả về HTTP 502.</summary>
public sealed class ExternalServiceException(string code, string message) : AppException(code, message);
