using Conduit.Features.Articles;
using FluentValidation.TestHelper;
using Xunit;

namespace Conduit.UnitTests.Features.Articles;

public class DetailsQueryValidatorTests
{
    private readonly Details.QueryValidator _validator;

    public DetailsQueryValidatorTests()
    {
        _validator = new Details.QueryValidator();
    }

    [Fact]
    public void Should_Have_Error_When_Slug_Is_Null()
    {
        // Arrange
        var query = new Details.Query(Slug: null!);

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Slug);
    }

    [Fact]
    public void Should_Have_Error_When_Slug_Is_Empty()
    {
        // Arrange
        var query = new Details.Query(Slug: "");

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Slug);
    }

    [Fact]
    public void Should_Not_Have_Error_When_Slug_Is_Valid()
    {
        // Arrange
        var query = new Details.Query(Slug: "test-article-slug");

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
