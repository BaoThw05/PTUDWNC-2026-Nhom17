using FluentValidation;

namespace CulinaryBlog.Application.Features.Auth.Logout;

public sealed class LogoutCommandValidator : AbstractValidator<LogoutCommand>
{
    private const int MaxTokenLength = 128;

    public LogoutCommandValidator()
    {
        RuleFor(command => command.RefreshToken).NotEmpty().MaximumLength(MaxTokenLength);
    }
}
