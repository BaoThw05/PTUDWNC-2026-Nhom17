namespace CulinaryBlog.API.Endpoints;

/// <summary>
/// Mỗi module cài đặt một lớp này; lớp được dò tự động nên không ai phải sửa Program.cs.
/// </summary>
public interface IEndpointModule
{
    /// <summary>Tên nhóm hiển thị trên Scalar.</summary>
    string Tag { get; }

    string Description { get; }

    /// <param name="api">Nhóm route đã có tiền tố <c>/api/v1</c>.</param>
    void MapEndpoints(IEndpointRouteBuilder api);
}
