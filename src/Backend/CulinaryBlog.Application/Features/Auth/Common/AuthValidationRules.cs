using FluentValidation;

namespace CulinaryBlog.Application.Features.Auth.Common;

internal static class AuthValidationRules
{
    public const int EmailMaxLength = 256;
    public const int FullNameMinLength = 2;
    public const int FullNameMaxLength = 100;
    public const int UserNameMinLength = 3;
    public const int UserNameMaxLength = 50;
    public const int PasswordMinLength = 8;
    public const int PasswordMaxLength = 128;

    public static IRuleBuilderOptions<T, string> ValidEmail<T>(this IRuleBuilder<T, string> rule) =>
        rule.NotEmpty().MaximumLength(EmailMaxLength).EmailAddress();

    public static IRuleBuilderOptions<T, string> ValidFullName<T>(this IRuleBuilder<T, string> rule) =>
        rule.NotEmpty()
            .Must(name => name.Trim().Length is >= FullNameMinLength and <= FullNameMaxLength)
            .WithMessage($"'{{PropertyName}}' must be between {FullNameMinLength} and {FullNameMaxLength} characters.");

    // SRS 3.1 (register): userName không chứa ký tự đặc biệt.
    public static IRuleBuilderOptions<T, string> ValidUserName<T>(this IRuleBuilder<T, string> rule) =>
        rule.NotEmpty()
            .MinimumLength(UserNameMinLength)
            .MaximumLength(UserNameMaxLength)
            .Matches("^[a-zA-Z0-9_.]+$")
            .WithMessage("'{PropertyName}' must contain only letters, digits, '_' or '.'.");

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
