using CulinaryBlog.Application.Features.Auth.Common;
using FluentValidation;

namespace CulinaryBlog.Application.Features.Auth.ChangeEmail;

public sealed class RequestEmailChangeCommandValidator : AbstractValidator<RequestEmailChangeCommand>
{
    public RequestEmailChangeCommandValidator()
    {
        RuleFor(command => command.NewEmail).ValidEmail();
        RuleFor(command => command.CurrentPassword).NotEmpty().MaximumLength(AuthValidationRules.PasswordMaxLength);
    }
}
