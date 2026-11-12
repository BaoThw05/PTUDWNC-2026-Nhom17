using CulinaryBlog.Application.Features.Auth.Common;
using FluentValidation;

namespace CulinaryBlog.Application.Features.Auth.ForgotPassword;

public sealed class ForgotPasswordCommandValidator : AbstractValidator<ForgotPasswordCommand>
{
    public ForgotPasswordCommandValidator()
    {
        RuleFor(command => command.Email).ValidEmail();
    }
}
