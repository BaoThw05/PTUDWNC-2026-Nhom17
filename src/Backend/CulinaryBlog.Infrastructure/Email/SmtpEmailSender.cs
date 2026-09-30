using CulinaryBlog.Application.Abstractions;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace CulinaryBlog.Infrastructure.Email;

public sealed class SmtpEmailSender(IOptions<EmailOptions> options, ILogger<SmtpEmailSender> logger) : IEmailSender
{
    private readonly EmailOptions _options = options.Value;

    public async Task SendAsync(string toEmail, string subject, string htmlBody, CancellationToken cancellationToken = default)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_options.SenderName, _options.SenderEmail));
        message.To.Add(MailboxAddress.Parse(toEmail));
        message.Subject = subject;
        message.Body = new BodyBuilder { HtmlBody = htmlBody }.ToMessageBody();

        using var client = new SmtpClient();

        // Mailhog dev không cần TLS/auth; port 465 (nếu dùng SMTP thật) mới bật SslOnConnect.
        var secureSocketOptions = _options.Port == 465
            ? SecureSocketOptions.SslOnConnect
            : SecureSocketOptions.StartTlsWhenAvailable;

        await client.ConnectAsync(_options.Host, _options.Port, secureSocketOptions, cancellationToken);

        if (!string.IsNullOrEmpty(_options.Username))
        {
            await client.AuthenticateAsync(_options.Username, _options.Password ?? string.Empty, cancellationToken);
        }

        await client.SendAsync(message, cancellationToken);
        await client.DisconnectAsync(true, cancellationToken);

        logger.LogInformation("Email sent to {Email} with subject {Subject}", toEmail, subject);
    }
}
