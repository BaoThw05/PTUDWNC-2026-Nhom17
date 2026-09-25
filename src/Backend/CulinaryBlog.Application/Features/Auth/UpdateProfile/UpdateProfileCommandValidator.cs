using CulinaryBlog.Application.Features.Auth.Common;
using FluentValidation;

namespace CulinaryBlog.Application.Features.Auth.UpdateProfile;

public sealed class UpdateProfileCommandValidator : AbstractValidator<UpdateProfileCommand>
{
    public UpdateProfileCommandValidator()
    {
        RuleFor(command => command.FullName).ValidFullName();
    }
}
