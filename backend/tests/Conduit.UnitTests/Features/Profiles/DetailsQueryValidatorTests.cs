using Conduit.Features.Profiles;
using FluentValidation.TestHelper;
using Xunit;

namespace Conduit.UnitTests.Features.Profiles;

public class DetailsQueryValidatorTests
{
    private readonly Details.QueryValidator _validator;

    public DetailsQueryValidatorTests()
    {
        _validator = new Details.QueryValidator();
    }

    [Fact]
    public void Should_Have_Error_When_Username_Is_Empty()
    {
        // Arrange
        var query = new Details.Query(Username: "");

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Username);
    }

    [Fact]
    public void Should_Not_Have_Error_When_Username_Is_Valid()
    {
        // Arrange
        var query = new Details.Query(Username: "testuser");

        // Act
        var result = _validator.TestValidate(query);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
