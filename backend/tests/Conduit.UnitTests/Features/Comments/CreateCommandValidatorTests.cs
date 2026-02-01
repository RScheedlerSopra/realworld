using Conduit.Features.Comments;
using FluentValidation.TestHelper;
using Xunit;

namespace Conduit.UnitTests.Features.Comments;

public class CreateCommandValidatorTests
{
    private readonly Create.CommandValidator _validator;

    public CreateCommandValidatorTests()
    {
        _validator = new Create.CommandValidator();
    }

    [Fact]
    public void Should_Have_Error_When_Body_Is_Null()
    {
        // Arrange
        var model = new Create.Model(new Create.CommentData(Body: null));
        var command = new Create.Command(model, "test-slug");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Model.Comment.Body);
    }

    [Fact]
    public void Should_Have_Error_When_Body_Is_Empty()
    {
        // Arrange
        var model = new Create.Model(new Create.CommentData(Body: ""));
        var command = new Create.Command(model, "test-slug");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Model.Comment.Body);
    }

    [Fact]
    public void Should_Not_Have_Error_When_Body_Is_Valid()
    {
        // Arrange
        var model = new Create.Model(new Create.CommentData(Body: "This is a valid comment"));
        var command = new Create.Command(model, "test-slug");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
