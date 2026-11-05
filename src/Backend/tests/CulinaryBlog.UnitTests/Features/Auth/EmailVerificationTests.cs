using CulinaryBlog.Application.Abstractions;
using CulinaryBlog.Application.Common.Exceptions;
using CulinaryBlog.Application.Features.Auth;
using CulinaryBlog.Application.Features.Auth.AccountEmail;
using CulinaryBlog.Application.Features.Auth.ChangeEmail;
using CulinaryBlog.Application.Features.Auth.ConfirmEmail;
using CulinaryBlog.UnitTests.Features.Auth.Fakes;
using Microsoft.Extensions.Logging.Abstractions;

namespace CulinaryBlog.UnitTests.Features.Auth;

public sealed class EmailVerificationTests
{
    private const string Email = "author@example.com";
    private const string Password = "Author@12345";
    private const string NewEmail = "new@example.com";

    private readonly AuthTestContext _context = new();
    private readonly FakeEmailSender _emailSender = new();
    private readonly AccountEmailJob _job;

    public EmailVerificationTests()
    {
        _job = new AccountEmailJob(
            _context.Users,
            _emailSender,
            new AppLinks("http://localhost:3000/"),
            NullLogger<AccountEmailJob>.Instance);
    }

    [Fact]
    public async Task ConfirmationJob_UnconfirmedUser_SendsLink()
    {
        var user = _context.Users.Add(Email, Password);

        await _job.SendConfirmationAsync(user.Id);

        var sent = Assert.Single(_emailSender.SentEmails);
        Assert.Equal(Email, sent.ToEmail);
        Assert.Contains($"http://localhost:3000/auth/confirm-email?userId={user.Id}&amp;token=confirm-{user.Id}", sent.HtmlBody);
    }

    [Fact]
    public async Task ConfirmationJob_AlreadyConfirmed_SendsNothing()
    {
        var user = _context.Users.Add(Email, Password);
        _context.Users.Replace(user with { EmailConfirmed = true });

        await _job.SendConfirmationAsync(user.Id);

        Assert.Empty(_emailSender.SentEmails);
    }

    [Fact]
    public async Task ConfirmEmail_ValidToken_MarksConfirmed_InvalidTokenThrows()
    {
        var user = _context.Users.Add(Email, Password);
        var handler = new ConfirmEmailCommandHandler(_context.Users);

        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => handler.Handle(new ConfirmEmailCommand(user.Id, "wrong"), CancellationToken.None));
        await handler.Handle(new ConfirmEmailCommand(user.Id, $"confirm-{user.Id}"), CancellationToken.None);

        Assert.Equal(AuthErrorCodes.EmailTokenInvalid, exception.Code);
        Assert.True((await _context.Users.FindByIdAsync(user.Id, CancellationToken.None))!.EmailConfirmed);
    }

    [Fact]
    public async Task ResendConfirmation_OnlyWhenUnconfirmed()
    {
        var user = _context.Users.Add(Email, Password);
        _context.CurrentUser.UserId = user.Id;
        var handler = new ResendEmailConfirmationCommandHandler(_context.CurrentUser, _context.Users, _context.Jobs);

        await handler.Handle(new ResendEmailConfirmationCommand(), CancellationToken.None);
        _context.Users.Replace(user with { EmailConfirmed = true });
        await handler.Handle(new ResendEmailConfirmationCommand(), CancellationToken.None);

        Assert.Single(_context.Jobs.Enqueued);
    }

    [Fact]
    public async Task RequestChange_CorrectPassword_EnqueuesCode()
    {
        var handler = CreateRequestHandler(_context.Users.Add(Email, Password));

        await handler.Handle(new RequestEmailChangeCommand($" {NewEmail} ", Password), CancellationToken.None);

        Assert.Single(_context.Jobs.Enqueued);
    }

    [Fact]
    public async Task RequestChange_WrongPassword_ThrowsAndSendsNothing()
    {
        var handler = CreateRequestHandler(_context.Users.Add(Email, Password));

        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => handler.Handle(new RequestEmailChangeCommand(NewEmail, "Wrong@12345"), CancellationToken.None));

        Assert.Contains("CurrentPassword", exception.Errors.Keys);
        Assert.Empty(_context.Jobs.Enqueued);
    }

    [Fact]
    public async Task RequestChange_LockedOut_ThrowsAccountLocked()
    {
        var user = _context.Users.Add(Email, Password);
        _context.Users.LockedOut.Add(user.Id);

        await Assert.ThrowsAsync<AccountLockedException>(
            () => CreateRequestHandler(user).Handle(new RequestEmailChangeCommand(NewEmail, Password), CancellationToken.None));
    }

    [Fact]
    public async Task RequestChange_EmailOfAnotherAccount_ThrowsConflict()
    {
        var handler = CreateRequestHandler(_context.Users.Add(Email, Password));
        _context.Users.Add(NewEmail, Password, userName: "other");

        var exception = await Assert.ThrowsAsync<ConflictException>(
            () => handler.Handle(new RequestEmailChangeCommand(NewEmail, Password), CancellationToken.None));

        Assert.Equal(AuthErrorCodes.EmailExists, exception.Code);
    }

    [Fact]
    public async Task RequestChange_SameEmail_ThrowsValidation()
    {
        var handler = CreateRequestHandler(_context.Users.Add(Email, Password));

        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => handler.Handle(new RequestEmailChangeCommand(Email.ToUpperInvariant(), Password), CancellationToken.None));

        Assert.Contains("NewEmail", exception.Errors.Keys);
    }

    [Fact]
    public async Task ChangeCodeJob_SendsCodeToNewEmail()
    {
        var user = _context.Users.Add(Email, Password);

        await _job.SendChangeEmailCodeAsync(user.Id, NewEmail);

        var sent = Assert.Single(_emailSender.SentEmails);
        Assert.Equal(NewEmail, sent.ToEmail);
        Assert.Contains(FakeUserAccountService.ChangeEmailCode, sent.HtmlBody);
    }

    [Fact]
    public async Task ConfirmChange_ValidCode_UpdatesEmail_InvalidCodeThrows()
    {
        var user = _context.Users.Add(Email, Password);
        _context.CurrentUser.UserId = user.Id;
        var handler = new ConfirmEmailChangeCommandHandler(_context.CurrentUser, _context.Users);

        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => handler.Handle(new ConfirmEmailChangeCommand(NewEmail, "000000"), CancellationToken.None));
        var profile = await handler.Handle(
            new ConfirmEmailChangeCommand(NewEmail, FakeUserAccountService.ChangeEmailCode),
            CancellationToken.None);

        Assert.Equal(AuthErrorCodes.EmailCodeInvalid, exception.Code);
        Assert.Equal(NewEmail, profile.Email);
        Assert.True(profile.EmailConfirmed);
    }

    private RequestEmailChangeCommandHandler CreateRequestHandler(Application.Features.Auth.Abstractions.UserAccount user)
    {
        _context.CurrentUser.UserId = user.Id;
        return new RequestEmailChangeCommandHandler(_context.CurrentUser, _context.Users, _context.Jobs);
    }
}
