using Conduit.Features.Articles;
using FluentValidation.TestHelper;
using Xunit;

namespace Conduit.UnitTests.Features.Articles;

public class PublishCommandValidatorTests
{
    private readonly Publish.CommandValidator _validator;

    public PublishCommandValidatorTests()
    {
        _validator = new Publish.CommandValidator();
    }

    [Fact]
    public void Should_HaveError_When_SlugIsNull()
    {
        // Arrange
        var command = new Publish.Command(null!);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Slug);
    }

    [Fact]
    public void Should_HaveError_When_SlugIsEmpty()
    {
        // Arrange
        var command = new Publish.Command(string.Empty);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Slug);
    }

    [Fact]
    public void Should_NotHaveError_When_SlugIsValid()
    {
        // Arrange
        var command = new Publish.Command("valid-slug");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Slug);
    }
}
