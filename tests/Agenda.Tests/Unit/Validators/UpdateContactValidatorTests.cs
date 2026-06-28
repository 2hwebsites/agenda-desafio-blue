using Agenda.Application.Contacts.Commands.UpdateContact;
using FluentValidation.TestHelper;
using Shouldly;

namespace Agenda.Tests.Unit.Validators;

public class UpdateContactValidatorTests
{
    private readonly UpdateContactValidator _validator = new();

    [Fact]
    public void Validate_ValidCommand_Passes()
    {
        var result = _validator.TestValidate(
            new UpdateContactCommand(Guid.NewGuid(), "Test User", "test@example.com", null));
        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public void Validate_EmptyId_FailsWithIdError()
    {
        var result = _validator.TestValidate(
            new UpdateContactCommand(Guid.Empty, "Test", "test@example.com", null));
        result.ShouldHaveValidationErrorFor(c => c.Id);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_EmptyName_FailsWithNameError(string name)
    {
        var result = _validator.TestValidate(
            new UpdateContactCommand(Guid.NewGuid(), name, "test@example.com", null));
        result.ShouldHaveValidationErrorFor(c => c.Name);
    }

    [Fact]
    public void Validate_NameTooLong_FailsWithNameError()
    {
        var result = _validator.TestValidate(
            new UpdateContactCommand(Guid.NewGuid(), new string('A', 151), "test@example.com", null));
        result.ShouldHaveValidationErrorFor(c => c.Name);
    }

    [Fact]
    public void Validate_InvalidEmailFormat_FailsWithEmailError()
    {
        var result = _validator.TestValidate(
            new UpdateContactCommand(Guid.NewGuid(), "Test", "invalid-email", null));
        result.ShouldHaveValidationErrorFor(c => c.Email);
    }

    [Fact]
    public void Validate_InvalidPhone_FailsWithPhoneError()
    {
        var result = _validator.TestValidate(
            new UpdateContactCommand(Guid.NewGuid(), "Test", "test@example.com", "bad"));
        result.ShouldHaveValidationErrorFor(c => c.Phone);
    }

    [Fact]
    public void Validate_NullPhone_Passes()
    {
        var result = _validator.TestValidate(
            new UpdateContactCommand(Guid.NewGuid(), "Test", "test@example.com", null));
        result.ShouldNotHaveValidationErrorFor(c => c.Phone);
    }
}
