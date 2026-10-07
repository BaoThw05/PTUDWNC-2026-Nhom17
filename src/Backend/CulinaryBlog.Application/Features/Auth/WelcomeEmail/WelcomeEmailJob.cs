using CulinaryBlog.Application.Abstractions;
using CulinaryBlog.Application.Features.Auth.Abstractions;
using Microsoft.Extensions.Logging;

namespace CulinaryBlog.Application.Features.Auth.WelcomeEmail;

public sealed class WelcomeEmailJob(
    IUserAccountService users,
    IEmailSender emailSender,
    ILogger<WelcomeEmailJob> logger) : IWelcomeEmailJob
{
    public async Task ExecuteAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await users.FindByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            logger.LogWarning("WelcomeEmailJob: user {UserId} không còn tồn tại, bỏ qua", userId);
            return;
        }

        const string subject = "Chào mừng bạn đến với Culinary Blog!";
        var htmlBody = $"""
            <h2>Xin chào {user.FullName},</h2>
            <p>Cảm ơn bạn đã đăng ký tài khoản tại Culinary Blog. Bạn có thể bắt đầu khám phá và chia sẻ công thức nấu ăn ngay bây giờ.</p>
            <p>Trân trọng,<br/>Culinary Blog Team</p>
            """;

        await emailSender.SendAsync(user.Email, subject, htmlBody, cancellationToken);
    }
}
