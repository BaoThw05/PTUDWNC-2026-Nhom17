using FluentValidation;

namespace CulinaryBlog.Application.Features.Auth.Refresh;

public sealed class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    private const int MaxTokenLength = 128;

    public RefreshTokenCommandValidator()
    {
        RuleFor(command => command.RefreshToken).NotEmpty().MaximumLength(MaxTokenLength);
    }
}
