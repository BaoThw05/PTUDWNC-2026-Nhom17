using CulinaryBlog.Application.Features.Auth.Common;
using FluentValidation;

namespace CulinaryBlog.Application.Features.Auth.ResetPassword;

public sealed class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
{
    public ResetPasswordCommandValidator()
    {
        RuleFor(command => command.UserId).NotEmpty();
        RuleFor(command => command.Token).NotEmpty().MaximumLength(2048);
        RuleFor(command => command.NewPassword).StrongPassword();
    }
}
