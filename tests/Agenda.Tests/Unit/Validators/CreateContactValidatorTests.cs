using Agenda.Application.Contacts.Commands.CreateContact;
using FluentValidation.TestHelper;
using Shouldly;

namespace Agenda.Tests.Unit.Validators;

public class CreateContactValidatorTests
{
    private readonly CreateContactValidator _validator = new();

    [Fact]
    public void Validate_ValidCommand_Passes()
    {
        var result = _validator.TestValidate(
            new CreateContactCommand("Test User", "test@example.com", "(11) 91234-5678"));
        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public void Validate_NullPhone_Passes()
    {
        var result = _validator.TestValidate(
            new CreateContactCommand("Test User", "test@example.com", null));
        result.IsValid.ShouldBeTrue();
    }

    [Theory]
    [InlineData("(11) 91234-5678")]
    [InlineData("(21) 98765-4321")]
    [InlineData("91234-5678")]
    [InlineData("11912345678")]
    public void Validate_ValidPhone_Passes(string phone)
    {
        var result = _validator.TestValidate(
            new CreateContactCommand("Test", "test@example.com", phone));
        result.ShouldNotHaveValidationErrorFor(c => c.Phone);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_EmptyName_FailsWithNameError(string name)
    {
        var result = _validator.TestValidate(
            new CreateContactCommand(name, "test@example.com", null));
        result.ShouldHaveValidationErrorFor(c => c.Name);
    }

    [Fact]
    public void Validate_NameTooLong_FailsWithNameError()
    {
        var result = _validator.TestValidate(
            new CreateContactCommand(new string('A', 151), "test@example.com", null));
        result.ShouldHaveValidationErrorFor(c => c.Name);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_EmptyEmail_FailsWithEmailError(string email)
    {
        var result = _validator.TestValidate(
            new CreateContactCommand("Test", email, null));
        result.ShouldHaveValidationErrorFor(c => c.Email);
    }

    [Fact]
    public void Validate_InvalidEmailFormat_FailsWithEmailError()
    {
        var result = _validator.TestValidate(
            new CreateContactCommand("Test", "not-an-email", null));
        result.ShouldHaveValidationErrorFor(c => c.Email);
    }

    [Fact]
    public void Validate_EmailTooLong_FailsWithEmailError()
    {
        var longEmail = new string('a', 246) + "@test.com"; // 255 chars > 254 limit
        var result = _validator.TestValidate(
            new CreateContactCommand("Test", longEmail, null));
        result.ShouldHaveValidationErrorFor(c => c.Email);
    }

    [Theory]
    [InlineData("123")]
    [InlineData("abc")]
    [InlineData("9999")]
    public void Validate_InvalidPhone_FailsWithPhoneError(string phone)
    {
        var result = _validator.TestValidate(
            new CreateContactCommand("Test", "test@example.com", phone));
        result.ShouldHaveValidationErrorFor(c => c.Phone);
    }
}
