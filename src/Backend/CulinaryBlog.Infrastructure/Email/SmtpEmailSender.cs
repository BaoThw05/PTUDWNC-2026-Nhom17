using CulinaryBlog.Application.Abstractions;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace CulinaryBlog.Infrastructure.Email;

/// <summary>
/// Gửi tới máy chủ chính (Mailpit khi dev) và, nếu có cấu hình Smtp:Gmail, đồng thời qua Gmail.
/// Hai đường độc lập: một bên lỗi chỉ ghi log. Chỉ ném lỗi khi mọi đường đều lỗi, để Hangfire
/// retry mà không gửi lặp mail tới người dùng khi đã có một đường thành công.
/// </summary>
public sealed class SmtpEmailSender(IOptions<EmailOptions> options, ILogger<SmtpEmailSender> logger) : IEmailSender
{
    private readonly EmailOptions _options = options.Value;

    public async Task SendAsync(string toEmail, string subject, string htmlBody, CancellationToken cancellationToken = default)
    {
        var sends = new List<Task<bool>>
        {
            TrySendAsync("Smtp", _options.Host, _options.Port, _options.Username, _options.Password,
                _options.SenderEmail, toEmail, subject, htmlBody, cancellationToken),
        };

        var gmail = _options.Gmail;
        if (gmail.IsConfigured)
        {
            sends.Add(TrySendAsync("Gmail", gmail.Host, gmail.Port, gmail.Username, gmail.Password,
                gmail.Username!, toEmail, subject, htmlBody, cancellationToken));
        }

        var results = await Task.WhenAll(sends);
        if (!results.Contains(true))
        {
            throw new InvalidOperationException($"Không gửi được email '{subject}' qua máy chủ SMTP nào.");
        }
    }

    private async Task<bool> TrySendAsync(
        string channel, string host, int port, string? username, string? password, string senderEmail,
        string toEmail, string subject, string htmlBody, CancellationToken cancellationToken)
    {
        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_options.SenderName, senderEmail));
            message.To.Add(MailboxAddress.Parse(toEmail));
            message.Subject = subject;
            message.Body = new BodyBuilder { HtmlBody = htmlBody }.ToMessageBody();

            using var client = new SmtpClient();

            // Mailpit dev không cần TLS/auth; port 465 bật SslOnConnect, cổng khác STARTTLS nếu máy chủ hỗ trợ.
            var secureSocketOptions = port == 465
                ? SecureSocketOptions.SslOnConnect
                : SecureSocketOptions.StartTlsWhenAvailable;

            await client.ConnectAsync(host, port, secureSocketOptions, cancellationToken);

            if (!string.IsNullOrEmpty(username))
            {
                await client.AuthenticateAsync(username, password ?? string.Empty, cancellationToken);
            }

            await client.SendAsync(message, cancellationToken);
            await client.DisconnectAsync(true, cancellationToken);

            logger.LogInformation("Email sent via {Channel} ({Host}:{Port}) to {Email} with subject {Subject}",
                channel, host, port, toEmail, subject);
            return true;
        }
        catch (Exception ex) when (!cancellationToken.IsCancellationRequested)
        {
            logger.LogWarning(ex, "Email via {Channel} ({Host}:{Port}) to {Email} failed", channel, host, port, toEmail);
            return false;
        }
    }
}
