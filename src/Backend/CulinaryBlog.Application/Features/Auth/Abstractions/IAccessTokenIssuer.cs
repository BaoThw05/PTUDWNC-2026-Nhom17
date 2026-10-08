namespace CulinaryBlog.Application.Features.Auth.Abstractions;

public interface IAccessTokenIssuer
{
    AccessToken Issue(UserAccount user);
}
