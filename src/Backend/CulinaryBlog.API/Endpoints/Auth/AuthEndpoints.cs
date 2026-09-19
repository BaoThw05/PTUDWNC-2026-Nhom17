namespace CulinaryBlog.API.Endpoints.Auth;

internal sealed class AuthEndpoints : IEndpointModule
{
    public string Tag => "Auth";

    public string Description => "Đăng ký, đăng nhập, refresh token, đăng xuất, Google, hồ sơ (TV1)";

    public void MapEndpoints(IEndpointRouteBuilder api)
    {
        var auth = api.MapGroup("/auth").WithTags(Tag);

        // TODO(TV1): map endpoint của module vào nhóm ở trên (FR-AUTH-001 → 007, FR-JOB-001).
    }
}
