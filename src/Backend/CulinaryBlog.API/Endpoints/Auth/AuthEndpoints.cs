using CulinaryBlog.API.Auth;
using CulinaryBlog.Application.Features.Auth.ChangeEmail;
using CulinaryBlog.Application.Features.Auth.ChangePassword;
using CulinaryBlog.Application.Features.Auth.Common;
using CulinaryBlog.Application.Features.Auth.ConfirmEmail;
using CulinaryBlog.Application.Features.Auth.ForgotPassword;
using CulinaryBlog.Application.Features.Auth.GetProfile;
using CulinaryBlog.Application.Features.Auth.GoogleLogin;
using CulinaryBlog.Application.Features.Auth.Login;
using CulinaryBlog.Application.Features.Auth.LoginHistory;
using CulinaryBlog.Application.Features.Auth.Logout;
using CulinaryBlog.Application.Features.Auth.Refresh;
using CulinaryBlog.Application.Features.Auth.Register;
using CulinaryBlog.Application.Features.Auth.ResetPassword;
using CulinaryBlog.Application.Features.Auth.UpdateProfile;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;

namespace CulinaryBlog.API.Endpoints.Auth;

internal sealed class AuthEndpoints : IEndpointModule
{
    private const string ProfilePath = $"{EndpointModuleExtensions.ApiPrefix}/auth/me";

    public string Tag => "Auth";

    public string Description => "Đăng ký, đăng nhập, refresh token, đăng xuất, Google, hồ sơ (TV1)";

    public void MapEndpoints(IEndpointRouteBuilder api)
    {
        var auth = api.MapGroup("/auth").WithTags(Tag);

        auth.MapPost("/register", RegisterAsync)
            .RequireRateLimiting(AuthRateLimitPolicies.Credentials)
            .WithSummary("Đăng ký tài khoản Author và đăng nhập luôn (FR-AUTH-001)")
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        auth.MapPost("/login", LoginAsync)
            .RequireRateLimiting(AuthRateLimitPolicies.Credentials)
            .WithSummary("Đăng nhập bằng email và mật khẩu (FR-AUTH-002)")
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity)
            .ProducesProblem(StatusCodes.Status423Locked);

        auth.MapPost("/google", GoogleLoginAsync)
            .RequireRateLimiting(AuthRateLimitPolicies.Credentials)
            .WithSummary("Đăng nhập bằng id_token của Google (FR-AUTH-003)")
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status502BadGateway);

        auth.MapPost("/refresh", RefreshAsync)
            .RequireRateLimiting(AuthRateLimitPolicies.Refresh)
            .WithSummary("Đổi refresh token lấy cặp token mới (FR-AUTH-004)")
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        auth.MapPost("/logout", LogoutAsync)
            .AllowAnonymous()
            .WithSummary("Thu hồi refresh token (FR-AUTH-005)");

        auth.MapGet("/me", GetProfileAsync)
            .RequireAuthorization()
            .WithSummary("Xem hồ sơ người đang đăng nhập (FR-AUTH-006)")
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound);

        auth.MapGet("/me/logins", GetLoginHistoryAsync)
            .RequireAuthorization()
            .WithSummary("20 lần đăng nhập gần nhất: thời gian, IP, thiết bị, kết quả")
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        auth.MapPost("/change-password", ChangePasswordAsync)
            .RequireAuthorization()
            .RequireRateLimiting(AuthRateLimitPolicies.Credentials)
            .WithSummary("Đổi mật khẩu và thu hồi mọi phiên đăng nhập")
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity)
            .ProducesProblem(StatusCodes.Status423Locked);

        auth.MapPost("/forgot-password", ForgotPasswordAsync)
            .RequireRateLimiting(AuthRateLimitPolicies.Credentials)
            .WithSummary("Gửi email đặt lại mật khẩu (luôn trả 204)")
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        auth.MapPost("/reset-password", ResetPasswordAsync)
            .RequireRateLimiting(AuthRateLimitPolicies.Credentials)
            .WithSummary("Đặt lại mật khẩu bằng token trong email và thu hồi mọi phiên đăng nhập")
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        auth.MapPost("/confirm-email", ConfirmEmailAsync)
            .RequireRateLimiting(AuthRateLimitPolicies.Credentials)
            .WithSummary("Xác nhận email bằng token trong link đã gửi")
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        auth.MapPost("/me/email-confirmation", ResendEmailConfirmationAsync)
            .RequireAuthorization()
            .RequireRateLimiting(AuthRateLimitPolicies.Credentials)
            .WithSummary("Gửi lại email xác nhận (không làm gì nếu email đã xác nhận)")
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        auth.MapPost("/me/email-change", RequestEmailChangeAsync)
            .RequireAuthorization()
            .RequireRateLimiting(AuthRateLimitPolicies.Credentials)
            .WithSummary("Bước 1 đổi email: kiểm tra mật khẩu, gửi mã 6 chữ số tới email mới")
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity)
            .ProducesProblem(StatusCodes.Status423Locked);

        auth.MapPost("/me/email-change/confirm", ConfirmEmailChangeAsync)
            .RequireAuthorization()
            .RequireRateLimiting(AuthRateLimitPolicies.Credentials)
            .WithSummary("Bước 2 đổi email: nhập mã đã gửi tới email mới")
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        auth.MapPatch("/me", UpdateProfileAsync)
            .RequireAuthorization()
            .WithSummary("Đổi tên hiển thị (FR-AUTH-007)")
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity);
    }

    private static async Task<Created<AuthResponse>> RegisterAsync(
        RegisterCommand command,
        ISender sender,
        CancellationToken cancellationToken) =>
        TypedResults.Created(ProfilePath, await sender.Send(command, cancellationToken));

    private static async Task<Ok<AuthResponse>> LoginAsync(
        LoginCommand command,
        ISender sender,
        CancellationToken cancellationToken) =>
        TypedResults.Ok(await sender.Send(command, cancellationToken));

    private static async Task<Ok<AuthResponse>> GoogleLoginAsync(
        GoogleLoginCommand command,
        ISender sender,
        CancellationToken cancellationToken) =>
        TypedResults.Ok(await sender.Send(command, cancellationToken));

    private static async Task<Ok<AuthResponse>> RefreshAsync(
        RefreshTokenCommand command,
        ISender sender,
        CancellationToken cancellationToken) =>
        TypedResults.Ok(await sender.Send(command, cancellationToken));

    private static async Task<NoContent> LogoutAsync(
        LogoutCommand command,
        ISender sender,
        CancellationToken cancellationToken)
    {
        await sender.Send(command, cancellationToken);
        return TypedResults.NoContent();
    }

    private static async Task<Ok<UserProfileResponse>> GetProfileAsync(
        ISender sender,
        CancellationToken cancellationToken) =>
        TypedResults.Ok(await sender.Send(new GetProfileQuery(), cancellationToken));

    private static async Task<Ok<IReadOnlyList<LoginAttemptResponse>>> GetLoginHistoryAsync(
        ISender sender,
        CancellationToken cancellationToken) =>
        TypedResults.Ok(await sender.Send(new GetLoginHistoryQuery(), cancellationToken));

    private static async Task<NoContent> ChangePasswordAsync(
        ChangePasswordCommand command,
        ISender sender,
        CancellationToken cancellationToken)
    {
        await sender.Send(command, cancellationToken);
        return TypedResults.NoContent();
    }

    private static async Task<NoContent> ForgotPasswordAsync(
        ForgotPasswordCommand command,
        ISender sender,
        CancellationToken cancellationToken)
    {
        await sender.Send(command, cancellationToken);
        return TypedResults.NoContent();
    }

    private static async Task<NoContent> ResetPasswordAsync(
        ResetPasswordCommand command,
        ISender sender,
        CancellationToken cancellationToken)
    {
        await sender.Send(command, cancellationToken);
        return TypedResults.NoContent();
    }

    private static async Task<NoContent> ConfirmEmailAsync(
        ConfirmEmailCommand command,
        ISender sender,
        CancellationToken cancellationToken)
    {
        await sender.Send(command, cancellationToken);
        return TypedResults.NoContent();
    }

    private static async Task<NoContent> ResendEmailConfirmationAsync(ISender sender, CancellationToken cancellationToken)
    {
        await sender.Send(new ResendEmailConfirmationCommand(), cancellationToken);
        return TypedResults.NoContent();
    }

    private static async Task<NoContent> RequestEmailChangeAsync(
        RequestEmailChangeCommand command,
        ISender sender,
        CancellationToken cancellationToken)
    {
        await sender.Send(command, cancellationToken);
        return TypedResults.NoContent();
    }

    private static async Task<Ok<UserProfileResponse>> ConfirmEmailChangeAsync(
        ConfirmEmailChangeCommand command,
        ISender sender,
        CancellationToken cancellationToken) =>
        TypedResults.Ok(await sender.Send(command, cancellationToken));

    private static async Task<Ok<UserProfileResponse>> UpdateProfileAsync(
        UpdateProfileCommand command,
        ISender sender,
        CancellationToken cancellationToken) =>
        TypedResults.Ok(await sender.Send(command, cancellationToken));
}
