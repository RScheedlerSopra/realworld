using Conduit.Features.Users;
using FluentValidation.TestHelper;
using Xunit;

namespace Conduit.UnitTests.Features.Users;

public class EditCommandValidatorTests
{
    private readonly Edit.CommandValidator _validator;

    public EditCommandValidatorTests()
    {
        _validator = new Edit.CommandValidator();
    }

    [Fact]
    public void Should_Have_Error_When_User_Is_Null()
    {
        // Arrange
        var command = new Edit.Command(User: null!);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.User);
    }

    [Fact]
    public void Should_Not_Have_Error_When_User_Is_Valid()
    {
        // Arrange
        var command = new Edit.Command(
            User: new Edit.UserData
            {
                Username = "testuser",
                Email = "test@example.com",
                Password = "newpassword",
                Bio = "Test bio",
                Image = "http://example.com/image.jpg"
            }
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Not_Have_Error_When_Only_Some_Fields_Provided()
    {
        // Arrange
        var command = new Edit.Command(
            User: new Edit.UserData
            {
                Username = null,
                Email = "test@example.com",
                Password = null,
                Bio = null,
                Image = null
            }
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
