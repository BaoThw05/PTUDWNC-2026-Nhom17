using CulinaryBlog.Application.Features.Auth.Register;

namespace CulinaryBlog.UnitTests.Features.Auth;

public sealed class RegisterCommandValidatorTests
{
    private readonly RegisterCommandValidator _validator = new();

    [Fact]
    public void Validate_ValidCommand_Passes()
    {
        var result = _validator.Validate(new RegisterCommand("Tuấn", "user@example.com", "tuan99", "Author@12345"));

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
        var result = _validator.Validate(new RegisterCommand("Tuấn", "user@example.com", "tuan99", password));

        Assert.Contains(result.Errors, error => error.PropertyName == nameof(RegisterCommand.Password));
    }

    [Fact]
    public void Validate_PasswordAtMinimumLength_Passes()
    {
        var result = _validator.Validate(new RegisterCommand("Tuấn", "user@example.com", "tuan99", "Sh0rt!ab"));

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData("A")]
    [InlineData("   ")]
    public void Validate_InvalidFullName_FailsOnFullName(string fullName)
    {
        var result = _validator.Validate(new RegisterCommand(fullName, "user@example.com", "tuan99", "Author@12345"));

        Assert.Contains(result.Errors, error => error.PropertyName == nameof(RegisterCommand.FullName));
    }

    [Fact]
    public void Validate_InvalidEmail_FailsOnEmail()
    {
        var result = _validator.Validate(new RegisterCommand("Tuấn", "not-an-email", "tuan99", "Author@12345"));

        Assert.Contains(result.Errors, error => error.PropertyName == nameof(RegisterCommand.Email));
    }

    [Theory]
    [InlineData("")]
    [InlineData("ab")]
    [InlineData("tuan 99")]
    [InlineData("tuan@99")]
    public void Validate_InvalidUserName_FailsOnUserName(string userName)
    {
        var result = _validator.Validate(new RegisterCommand("Tuấn", "user@example.com", userName, "Author@12345"));

        Assert.Contains(result.Errors, error => error.PropertyName == nameof(RegisterCommand.UserName));
    }
}
