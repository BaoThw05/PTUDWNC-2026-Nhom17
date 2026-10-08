using CulinaryBlog.Application.Features.Auth.Common;
using MediatR;

namespace CulinaryBlog.Application.Features.Auth.GetProfile;

public sealed record GetProfileQuery : IRequest<UserProfileResponse>;
