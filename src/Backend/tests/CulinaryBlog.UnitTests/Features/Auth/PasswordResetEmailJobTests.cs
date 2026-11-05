using CulinaryBlog.Application.Abstractions;
using CulinaryBlog.Application.Features.Auth.PasswordResetEmail;
using CulinaryBlog.UnitTests.Features.Auth.Fakes;
using Microsoft.Extensions.Logging.Abstractions;

namespace CulinaryBlog.UnitTests.Features.Auth;

public sealed class PasswordResetEmailJobTests
{
    private readonly FakeUserAccountService _users = new();
    private readonly FakeEmailSender _emailSender = new();
    private readonly PasswordResetEmailJob _job;

    public PasswordResetEmailJobTests()
    {
        _job = new PasswordResetEmailJob(
            _users,
            _emailSender,
            new AppLinks("http://localhost:3000/"),
            NullLogger<PasswordResetEmailJob>.Instance);
    }

    [Fact]
    public async Task ExecuteAsync_ActiveUser_SendsEmailWithResetLink()
    {
        var user = _users.Add("cook@example.com", "irrelevant");

        await _job.ExecuteAsync(user.Id, CancellationToken.None);

        var sent = Assert.Single(_emailSender.SentEmails);
        Assert.Equal(user.Email, sent.ToEmail);
        Assert.Contains("http://localhost:3000/auth/reset-password?userId=" + user.Id, sent.HtmlBody);
        Assert.Contains($"token=reset-{user.Id}", sent.HtmlBody);
    }

    [Fact]
    public async Task ExecuteAsync_DisabledOrUnknownUser_DoesNotSendEmail()
    {
        var disabled = _users.Add("off@example.com", "irrelevant", isActive: false);

        await _job.ExecuteAsync(disabled.Id, CancellationToken.None);
        await _job.ExecuteAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.Empty(_emailSender.SentEmails);
    }
}
