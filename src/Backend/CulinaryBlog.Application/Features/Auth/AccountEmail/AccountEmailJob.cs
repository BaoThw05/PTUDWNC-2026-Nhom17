using System.Net;
using CulinaryBlog.Application.Abstractions;
using CulinaryBlog.Application.Features.Auth.Abstractions;
using Microsoft.Extensions.Logging;

namespace CulinaryBlog.Application.Features.Auth.AccountEmail;

/// <summary>Token và mã OTP sinh ngay trong job (không truyền qua Hangfire) để không bị lưu trong dữ liệu job.</summary>
public sealed class AccountEmailJob(
    IUserAccountService users,
    IEmailSender emailSender,
    AppLinks links,
    ILogger<AccountEmailJob> logger) : IAccountEmailJob
{
    public async Task SendConfirmationAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await users.FindByIdAsync(userId, cancellationToken);
        if (user is not { IsActive: true, EmailConfirmed: false })
        {
            logger.LogInformation("AccountEmailJob: user {UserId} không cần xác nhận email, bỏ qua", userId);
            return;
        }

        var token = await users.CreateEmailConfirmationTokenAsync(userId, cancellationToken);
        var link = WebUtility.HtmlEncode(
            $"{links.FrontendBaseUrl.TrimEnd('/')}/auth/confirm-email?userId={userId}&token={Uri.EscapeDataString(token)}");

        var htmlBody = $"""
            <h2>Xin chào {WebUtility.HtmlEncode(user.FullName)},</h2>
            <p>Hãy xác nhận địa chỉ email của bạn để bảo vệ tài khoản Culinary Blog.</p>
            <p><a href="{link}">Xác nhận email</a> (liên kết có hiệu lực trong 1 ngày).</p>
            <p>Nếu bạn không tạo tài khoản này, hãy bỏ qua email.</p>
            <p>Trân trọng,<br/>Culinary Blog Team</p>
            """;

        await emailSender.SendAsync(user.Email, "Xác nhận email Culinary Blog", htmlBody, cancellationToken);
    }

    public async Task SendChangeEmailCodeAsync(Guid userId, string newEmail, CancellationToken cancellationToken = default)
    {
        var user = await users.FindByIdAsync(userId, cancellationToken);
        if (user is not { IsActive: true })
        {
            logger.LogWarning("AccountEmailJob: user {UserId} không còn hoạt động, bỏ qua", userId);
            return;
        }

        var code = await users.CreateChangeEmailCodeAsync(userId, newEmail, cancellationToken);

        var htmlBody = $"""
            <h2>Xin chào {WebUtility.HtmlEncode(user.FullName)},</h2>
            <p>Mã xác minh để đổi email tài khoản Culinary Blog sang địa chỉ này:</p>
            <p style="font-size:24px;font-weight:bold;letter-spacing:4px">{code}</p>
            <p>Mã có hiệu lực trong vài phút. Nếu bạn không yêu cầu, hãy bỏ qua email này.</p>
            <p>Trân trọng,<br/>Culinary Blog Team</p>
            """;

        await emailSender.SendAsync(newEmail, "Mã xác minh đổi email Culinary Blog", htmlBody, cancellationToken);
    }
}
