using CulinaryBlog.Application.Abstractions;

namespace CulinaryBlog.UnitTests.Features.Auth.Fakes;

internal sealed class FakeCurrentUser : ICurrentUser
{
    public Guid? UserId { get; set; }

    public string? IpAddress { get; set; } = "127.0.0.1";

    public string? UserAgent { get; set; } = "UnitTest/1.0";

    public bool IsInRole(string role) => false;
}
