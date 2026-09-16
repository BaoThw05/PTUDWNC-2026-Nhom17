using CulinaryBlog.Application.Features.Auth.Register;

namespace CulinaryBlog.UnitTests.Features.Auth;

public sealed class RegisterCommandValidatorTests
{
    private readonly RegisterCommandValidator _validator = new();

    [Fact]
    public void Validate_ValidCommand_Passes()
    {
        var result = _validator.Validate(new RegisterCommand("user@example.com", "Author@12345", "Tuấn"));

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("Sh0rt!a")]
    [InlineData("alllowercase1!")]
    [InlineData("ALLUPPERCASE1!")]
    [InlineData("NoDigits!!")]
    [InlineData("NoSpecial123")]
    public void Validate_WeakPassword_FailsOnPassword(string password)
    {
        var result = _validator.Validate(new RegisterCommand("user@example.com", password, "Tuấn"));

        Assert.Contains(result.Errors, error => error.PropertyName == nameof(RegisterCommand.Password));
    }

    [Fact]
    public void Validate_PasswordAtMinimumLength_Passes()
    {
        var result = _validator.Validate(new RegisterCommand("user@example.com", "Sh0rt!ab", "Tuấn"));

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData("A")]
    [InlineData("   ")]
    public void Validate_InvalidDisplayName_FailsOnDisplayName(string displayName)
    {
        var result = _validator.Validate(new RegisterCommand("user@example.com", "Author@12345", displayName));

        Assert.Contains(result.Errors, error => error.PropertyName == nameof(RegisterCommand.DisplayName));
    }

    [Fact]
    public void Validate_InvalidEmail_FailsOnEmail()
    {
        var result = _validator.Validate(new RegisterCommand("not-an-email", "Author@12345", "Tuấn"));

        Assert.Contains(result.Errors, error => error.PropertyName == nameof(RegisterCommand.Email));
    }
}
