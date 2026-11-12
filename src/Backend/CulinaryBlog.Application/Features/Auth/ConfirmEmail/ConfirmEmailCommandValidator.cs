using FluentValidation;

namespace CulinaryBlog.Application.Features.Auth.ConfirmEmail;

public sealed class ConfirmEmailCommandValidator : AbstractValidator<ConfirmEmailCommand>
{
    public ConfirmEmailCommandValidator()
    {
        RuleFor(command => command.UserId).NotEmpty();
        RuleFor(command => command.Token).NotEmpty().MaximumLength(2048);
    }
}
