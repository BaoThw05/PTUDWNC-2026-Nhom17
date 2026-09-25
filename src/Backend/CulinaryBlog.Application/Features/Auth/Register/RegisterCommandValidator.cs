using CulinaryBlog.Application.Features.Auth.Common;
using FluentValidation;

namespace CulinaryBlog.Application.Features.Auth.Register;

public sealed class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(command => command.FullName).ValidFullName();
        RuleFor(command => command.Email).ValidEmail();
        RuleFor(command => command.UserName).ValidUserName();
        RuleFor(command => command.Password).StrongPassword();
    }
}
