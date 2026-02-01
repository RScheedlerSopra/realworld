using Conduit.Features.Articles;
using FluentValidation.TestHelper;
using Xunit;

namespace Conduit.UnitTests.Features.Articles;

public class CreateCommandValidatorTests
{
    private readonly Create.CommandValidator _validator;

    public CreateCommandValidatorTests()
    {
        _validator = new Create.CommandValidator();
    }

    [Fact]
    public void Should_Have_Error_When_Article_Is_Null()
    {
        // Arrange
        var command = new Create.Command(Article: null!);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Article);
    }

    [Fact]
    public void Should_Have_Error_When_Article_Title_Is_Null()
    {
        // Arrange
        var command = new Create.Command(
            Article: new Create.ArticleData
            {
                Title = null,
                Description = "Test description",
                Body = "Test body"
            }
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor("Article.Title");
    }

    [Fact]
    public void Should_Not_Have_Error_When_Article_Is_Valid()
    {
        // Arrange
        var command = new Create.Command(
            Article: new Create.ArticleData
            {
                Title = "Test title",
                Description = "Test description",
                Body = "Test body",
                TagList = ["tag1", "tag2"]
            }
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
