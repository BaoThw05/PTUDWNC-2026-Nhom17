using CulinaryBlog.Application.Features.Auth.WelcomeEmail;
using CulinaryBlog.UnitTests.Features.Auth.Fakes;
using Microsoft.Extensions.Logging.Abstractions;

namespace CulinaryBlog.UnitTests.Features.Auth;

public sealed class WelcomeEmailJobTests
{
    private readonly FakeUserAccountService _users = new();
    private readonly FakeEmailSender _emailSender = new();
    private readonly WelcomeEmailJob _job;

    public WelcomeEmailJobTests()
    {
        _job = new WelcomeEmailJob(_users, _emailSender, NullLogger<WelcomeEmailJob>.Instance);
    }

    [Fact]
    public async Task ExecuteAsync_ExistingUser_SendsHtmlEmailWithFullName()
    {
        var user = _users.Add("cook@example.com", "irrelevant");

        await _job.ExecuteAsync(user.Id, CancellationToken.None);

        var sent = Assert.Single(_emailSender.SentEmails);
        Assert.Equal(user.Email, sent.ToEmail);
        Assert.Contains(user.FullName, sent.HtmlBody);
    }

    [Fact]
    public async Task ExecuteAsync_UnknownUser_DoesNotSendEmail()
    {
        await _job.ExecuteAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.Empty(_emailSender.SentEmails);
    }
}
