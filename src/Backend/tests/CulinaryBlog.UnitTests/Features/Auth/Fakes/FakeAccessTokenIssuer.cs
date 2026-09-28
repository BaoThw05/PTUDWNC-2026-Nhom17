using CulinaryBlog.Application.Features.Auth.Abstractions;

namespace CulinaryBlog.UnitTests.Features.Auth.Fakes;

internal sealed class FakeAccessTokenIssuer(TimeProvider timeProvider) : IAccessTokenIssuer
{
    public AccessToken Issue(UserAccount user) =>
        new($"access-{user.Id}", timeProvider.GetUtcNow().AddMinutes(15));
}
