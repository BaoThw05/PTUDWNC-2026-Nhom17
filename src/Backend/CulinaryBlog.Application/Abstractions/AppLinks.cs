namespace CulinaryBlog.Application.Abstractions;

/// <summary>Địa chỉ gốc của frontend, dùng để tạo link trong email (cấu hình <c>Frontend:BaseUrl</c>).</summary>
public sealed record AppLinks(string FrontendBaseUrl);