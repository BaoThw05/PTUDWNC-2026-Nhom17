using CulinaryBlog.Application.Features.Auth.Common;
using FluentValidation;

namespace CulinaryBlog.Application.Features.Auth.ChangeEmail;

public sealed class ConfirmEmailChangeCommandValidator : AbstractValidator<ConfirmEmailChangeCommand>
{
    public ConfirmEmailChangeCommandValidator()
    {
        RuleFor(command => command.NewEmail).ValidEmail();
        RuleFor(command => command.Code).NotEmpty().Matches("^[0-9]{6}$")
            .WithMessage("'{PropertyName}' must be 6 digits.");
    }
}
