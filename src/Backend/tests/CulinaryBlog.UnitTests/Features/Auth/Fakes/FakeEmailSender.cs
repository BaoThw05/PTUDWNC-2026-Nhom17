using CulinaryBlog.Application.Abstractions;

namespace CulinaryBlog.UnitTests.Features.Auth.Fakes;

internal sealed class FakeEmailSender : IEmailSender
{
    public List<(string ToEmail, string Subject, string HtmlBody)> SentEmails { get; } = [];

    public Task SendAsync(string toEmail, string subject, string htmlBody, CancellationToken cancellationToken = default)
    {
        SentEmails.Add((toEmail, subject, htmlBody));
        return Task.CompletedTask;
    }
}
