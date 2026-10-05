using CulinaryBlog.Application.Common.Models;
using CulinaryBlog.Application.Features.Auth.Abstractions;
using MediatR;

namespace CulinaryBlog.Application.Features.Auth.AdminUsers;

public sealed record ListUsersQuery(string? Search, int Page = 1, int PageSize = ListUsersQuery.DefaultPageSize)
    : IRequest<PagedResult<AdminUserResponse>>
{
    public const int DefaultPageSize = 20;
}

public sealed class ListUsersQueryHandler(IUserAccountService users)
    : IRequestHandler<ListUsersQuery, PagedResult<AdminUserResponse>>
{
    public async Task<PagedResult<AdminUserResponse>> Handle(ListUsersQuery request, CancellationToken cancellationToken)
    {
        var page = Math.Max(request.Page, 1);
        var pageSize = Math.Clamp(request.PageSize, 1, PagedResult<AdminUserResponse>.MaxPageSize);
        var search = string.IsNullOrWhiteSpace(request.Search) ? null : request.Search.Trim();

        var result = await users.ListAsync(search, page, pageSize, cancellationToken);

        return new PagedResult<AdminUserResponse>(
            [.. result.Items.Select(AdminUserResponse.From)],
            result.Page,
            result.PageSize,
            result.TotalCount);
    }
}
