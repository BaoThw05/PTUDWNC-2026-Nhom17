using FluentValidation;

namespace CulinaryBlog.Application.Features.Auth.GoogleLogin;

public sealed class GoogleLoginCommandValidator : AbstractValidator<GoogleLoginCommand>
{
    private const int MaxIdTokenLength = 4096;

    public GoogleLoginCommandValidator()
    {
        RuleFor(command => command.IdToken).NotEmpty().MaximumLength(MaxIdTokenLength);
    }
}
