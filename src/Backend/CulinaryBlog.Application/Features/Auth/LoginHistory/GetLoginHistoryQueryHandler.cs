using CulinaryBlog.Application.Abstractions;
using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Features.Auth.Abstractions;
using MediatR;

namespace CulinaryBlog.Application.Features.Auth.LoginHistory;

public sealed class GetLoginHistoryQueryHandler(ICurrentUser currentUser, ILoginAttemptRepository attempts)
    : IRequestHandler<GetLoginHistoryQuery, IReadOnlyList<LoginAttemptResponse>>
{
    // shortcut: chỉ trả 20 lần gần nhất, không phân trang; thêm page/pageSize khi giao diện cần xem lâu hơn.
    public const int Take = 20;

    public async Task<IReadOnlyList<LoginAttemptResponse>> Handle(
        GetLoginHistoryQuery request,
        CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId ?? throw new UnauthorizedException("Authentication is required.");
        var recent = await attempts.ListRecentAsync(userId, Take, cancellationToken);
        return [.. recent.Select(LoginAttemptResponse.From)];
    }
}
