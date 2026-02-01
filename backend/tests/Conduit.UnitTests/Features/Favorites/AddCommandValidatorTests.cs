using Conduit.Features.Favorites;
using FluentValidation.TestHelper;
using Xunit;

namespace Conduit.UnitTests.Features.Favorites;

public class AddCommandValidatorTests
{
    private readonly Add.CommandValidator _validator;

    public AddCommandValidatorTests()
    {
        _validator = new Add.CommandValidator();
    }

    [Fact]
    public void Should_Have_Error_When_Slug_Is_Null()
    {
        // Arrange
        var command = new Add.Command(Slug: null!);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Slug);
    }

    [Fact]
    public void Should_Have_Error_When_Slug_Is_Empty()
    {
        // Arrange
        var command = new Add.Command(Slug: "");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Slug);
    }

    [Fact]
    public void Should_Not_Have_Error_When_Slug_Is_Valid()
    {
        // Arrange
        var command = new Add.Command(Slug: "test-slug");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
