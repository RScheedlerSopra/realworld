using Conduit.Features.Followers;
using FluentValidation.TestHelper;
using Xunit;

namespace Conduit.UnitTests.Features.Followers;

public class AddCommandValidatorTests
{
    private readonly Add.CommandValidator _validator;

    public AddCommandValidatorTests()
    {
        _validator = new Add.CommandValidator();
    }

    [Fact]
    public void Should_Have_Error_When_Username_Is_Null()
    {
        // Arrange
        var command = new Add.Command(Username: null!);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Username);
    }

    [Fact]
    public void Should_Have_Error_When_Username_Is_Empty()
    {
        // Arrange
        var command = new Add.Command(Username: "");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Username);
    }

    [Fact]
    public void Should_Not_Have_Error_When_Username_Is_Valid()
    {
        // Arrange
        var command = new Add.Command(Username: "testuser");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
