using FluentValidation;

namespace CulinaryBlog.Application.Features.Auth.Common;

internal static class AuthValidationRules
{
    public const int EmailMaxLength = 256;
    public const int DisplayNameMinLength = 2;
    public const int DisplayNameMaxLength = 100;
    public const int PasswordMinLength = 8;
    public const int PasswordMaxLength = 128;

    public static IRuleBuilderOptions<T, string> ValidEmail<T>(this IRuleBuilder<T, string> rule) =>
        rule.NotEmpty().MaximumLength(EmailMaxLength).EmailAddress();

    public static IRuleBuilderOptions<T, string> ValidDisplayName<T>(this IRuleBuilder<T, string> rule) =>
        rule.NotEmpty()
            .Must(name => name.Trim().Length is >= DisplayNameMinLength and <= DisplayNameMaxLength)
            .WithMessage($"'{{PropertyName}}' must be between {DisplayNameMinLength} and {DisplayNameMaxLength} characters.");

    // NFR-SEC-001: ≥ 8 ký tự, có chữ hoa, chữ thường, số và ký tự đặc biệt.
    public static IRuleBuilderOptions<T, string> StrongPassword<T>(this IRuleBuilder<T, string> rule) =>
        rule.NotEmpty()
            .MinimumLength(PasswordMinLength)
            .MaximumLength(PasswordMaxLength)
            .Matches("[A-Z]").WithMessage("'{PropertyName}' must contain an uppercase letter.")
            .Matches("[a-z]").WithMessage("'{PropertyName}' must contain a lowercase letter.")
            .Matches("[0-9]").WithMessage("'{PropertyName}' must contain a digit.")
            .Matches("[^a-zA-Z0-9]").WithMessage("'{PropertyName}' must contain a special character.");
}
