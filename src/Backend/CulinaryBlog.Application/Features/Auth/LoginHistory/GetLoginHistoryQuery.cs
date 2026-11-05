using MediatR;

namespace CulinaryBlog.Application.Features.Auth.LoginHistory;

public sealed record GetLoginHistoryQuery : IRequest<IReadOnlyList<LoginAttemptResponse>>;
