using System.Net;
using CulinaryBlog.Application.Abstractions;
using CulinaryBlog.Application.Features.Auth.Abstractions;
using Microsoft.Extensions.Logging;

namespace CulinaryBlog.Application.Features.Auth.PasswordResetEmail;

public sealed class PasswordResetEmailJob(
    IUserAccountService users,
    IEmailSender emailSender,
    AppLinks links,
    ILogger<PasswordResetEmailJob> logger) : IPasswordResetEmailJob
{
    public async Task ExecuteAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await users.FindByIdAsync(userId, cancellationToken);
        if (user is not { IsActive: true })
        {
            logger.LogWarning("PasswordResetEmailJob: user {UserId} không còn hoạt động, bỏ qua", userId);
            return;
        }

        // Token sinh tại đây (không truyền qua Hangfire) để không bị lưu trong dữ liệu job.
        var token = await users.CreatePasswordResetTokenAsync(userId, cancellationToken);
        var link = WebUtility.HtmlEncode(
            $"{links.FrontendBaseUrl.TrimEnd('/')}/auth/reset-password?userId={userId}&token={Uri.EscapeDataString(token)}");

        var htmlBody = $"""
            <h2>Xin chào {WebUtility.HtmlEncode(user.FullName)},</h2>
            <p>Chúng tôi nhận được yêu cầu đặt lại mật khẩu cho tài khoản của bạn.</p>
            <p><a href="{link}">Đặt lại mật khẩu</a> (liên kết có hiệu lực trong 1 ngày, dùng được một lần).</p>
            <p>Nếu bạn không yêu cầu, hãy bỏ qua email này.</p>
            <p>Trân trọng,<br/>Culinary Blog Team</p>
            """;

        await emailSender.SendAsync(user.Email, "Đặt lại mật khẩu Culinary Blog", htmlBody, cancellationToken);
    }
}
