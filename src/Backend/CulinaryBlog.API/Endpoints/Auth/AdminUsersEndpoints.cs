using CulinaryBlog.API.Auth;
using CulinaryBlog.Application.Common.Models;
using CulinaryBlog.Application.Features.Auth.AdminUsers;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;

namespace CulinaryBlog.API.Endpoints.Auth;

internal sealed class AdminUsersEndpoints : IEndpointModule
{
    public string Tag => "Admin Users";

    public string Description => "Quản trị người dùng: danh sách, khóa/mở tài khoản, gán vai trò (TV1)";

    public void MapEndpoints(IEndpointRouteBuilder api)
    {
        var users = api.MapGroup("/admin/users").WithTags(Tag).RequireAuthorization(AuthPolicies.Admin);

        users.MapGet("/", ListAsync)
            .WithSummary("Danh sách người dùng, tìm theo email/tên đăng nhập/họ tên")
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        users.MapPatch("/{id:guid}", UpdateAsync)
            .WithSummary("Khóa/mở tài khoản và gán vai trò; khóa thì thu hồi mọi refresh token")
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity);
    }

    private static async Task<Ok<PagedResult<AdminUserResponse>>> ListAsync(
        string? search,
        int? page,
        int? pageSize,
        ISender sender,
        CancellationToken cancellationToken) =>
        TypedResults.Ok(await sender.Send(
            new ListUsersQuery(search, page ?? 1, pageSize ?? ListUsersQuery.DefaultPageSize),
            cancellationToken));

    private static async Task<Ok<AdminUserResponse>> UpdateAsync(
        Guid id,
        UpdateUserRequest body,
        ISender sender,
        CancellationToken cancellationToken) =>
        TypedResults.Ok(await sender.Send(new UpdateUserCommand(id, body.IsActive, body.Roles), cancellationToken));

    internal sealed record UpdateUserRequest(bool? IsActive, IReadOnlyList<string>? Roles);
}
