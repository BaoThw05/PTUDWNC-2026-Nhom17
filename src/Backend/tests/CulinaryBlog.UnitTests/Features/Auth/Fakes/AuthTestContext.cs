using CulinaryBlog.Application.Features.Auth.Common;

namespace CulinaryBlog.UnitTests.Features.Auth.Fakes;

/// <summary>Gom các fake dùng chung cho test handler của module Auth.</summary>
internal sealed class AuthTestContext
{
    public AuthTestContext()
    {
        Sessions = new AuthSessionIssuer(new FakeAccessTokenIssuer(Time), RefreshTokens, CurrentUser, Time);
    }

    public ManualTimeProvider Time { get; } = new(new DateTimeOffset(2026, 9, 20, 8, 0, 0, TimeSpan.Zero));

    public FakeUserAccountService Users { get; } = new();

    public InMemoryRefreshTokenRepository RefreshTokens { get; } = new();

    public FakeCurrentUser CurrentUser { get; } = new();

    public AuthSessionIssuer Sessions { get; }
}
