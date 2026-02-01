using Conduit.Features.Articles;
using FluentValidation.TestHelper;
using Xunit;

namespace Conduit.UnitTests.Features.Articles;

public class DeleteCommandValidatorTests
{
    private readonly Delete.CommandValidator _validator;

    public DeleteCommandValidatorTests()
    {
        _validator = new Delete.CommandValidator();
    }

    [Fact]
    public void Should_Have_Error_When_Slug_Is_Null()
    {
        // Arrange
        var command = new Delete.Command(Slug: null!);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Slug);
    }

    [Fact]
    public void Should_Have_Error_When_Slug_Is_Empty()
    {
        // Arrange
        var command = new Delete.Command(Slug: "");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Slug);
    }

    [Fact]
    public void Should_Not_Have_Error_When_Slug_Is_Valid()
    {
        // Arrange
        var command = new Delete.Command(Slug: "test-article-slug");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
