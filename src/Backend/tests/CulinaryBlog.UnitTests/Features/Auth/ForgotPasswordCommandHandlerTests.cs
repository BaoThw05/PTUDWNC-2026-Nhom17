using CulinaryBlog.Application.Features.Auth.ForgotPassword;
using CulinaryBlog.UnitTests.Features.Auth.Fakes;

namespace CulinaryBlog.UnitTests.Features.Auth;

public sealed class ForgotPasswordCommandHandlerTests
{
    private readonly AuthTestContext _context = new();
    private readonly ForgotPasswordCommandHandler _handler;

    public ForgotPasswordCommandHandlerTests()
    {
        _handler = new ForgotPasswordCommandHandler(_context.Users, _context.Jobs);
    }

    [Fact]
    public async Task Handle_ExistingActiveUser_EnqueuesResetEmail()
    {
        _context.Users.Add("cook@example.com", "irrelevant");

        await _handler.Handle(new ForgotPasswordCommand("COOK@example.com"), CancellationToken.None);

        Assert.Single(_context.Jobs.Enqueued);
    }

    [Fact]
    public async Task Handle_UnknownEmail_EnqueuesNothingAndDoesNotThrow()
    {
        await _handler.Handle(new ForgotPasswordCommand("nobody@example.com"), CancellationToken.None);

        Assert.Empty(_context.Jobs.Enqueued);
    }

    [Fact]
    public async Task Handle_DisabledUser_EnqueuesNothing()
    {
        _context.Users.Add("cook@example.com", "irrelevant", isActive: false);

        await _handler.Handle(new ForgotPasswordCommand("cook@example.com"), CancellationToken.None);

        Assert.Empty(_context.Jobs.Enqueued);
    }
}
