using Conduit.Features.Users;
using FluentValidation.TestHelper;
using Xunit;

namespace Conduit.UnitTests.Features.Users;

public class CreateCommandValidatorTests
{
    private readonly Create.CommandValidator _validator;

    public CreateCommandValidatorTests()
    {
        _validator = new Create.CommandValidator();
    }

    [Fact]
    public void Should_Have_Error_When_Username_Is_Null()
    {
        // Arrange
        var command = new Create.Command(
            User: new Create.UserData(Username: null, Email: "test@example.com", Password: "password")
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.User.Username);
    }

    [Fact]
    public void Should_Have_Error_When_Username_Is_Empty()
    {
        // Arrange
        var command = new Create.Command(
            User: new Create.UserData(Username: "", Email: "test@example.com", Password: "password")
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.User.Username);
    }

    [Fact]
    public void Should_Have_Error_When_Email_Is_Null()
    {
        // Arrange
        var command = new Create.Command(
            User: new Create.UserData(Username: "testuser", Email: null, Password: "password")
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.User.Email);
    }

    [Fact]
    public void Should_Have_Error_When_Email_Is_Empty()
    {
        // Arrange
        var command = new Create.Command(
            User: new Create.UserData(Username: "testuser", Email: "", Password: "password")
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.User.Email);
    }

    [Fact]
    public void Should_Have_Error_When_Password_Is_Null()
    {
        // Arrange
        var command = new Create.Command(
            User: new Create.UserData(Username: "testuser", Email: "test@example.com", Password: null)
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.User.Password);
    }

    [Fact]
    public void Should_Have_Error_When_Password_Is_Empty()
    {
        // Arrange
        var command = new Create.Command(
            User: new Create.UserData(Username: "testuser", Email: "test@example.com", Password: "")
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.User.Password);
    }

    [Fact]
    public void Should_Not_Have_Error_When_Command_Is_Valid()
    {
        // Arrange
        var command = new Create.Command(
            User: new Create.UserData(Username: "testuser", Email: "test@example.com", Password: "password")
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
