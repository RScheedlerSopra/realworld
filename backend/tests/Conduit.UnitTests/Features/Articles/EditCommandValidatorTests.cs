using Conduit.Features.Articles;
using FluentValidation.TestHelper;
using Xunit;

namespace Conduit.UnitTests.Features.Articles;

public class EditCommandValidatorTests
{
    private readonly Edit.CommandValidator _validator;

    public EditCommandValidatorTests()
    {
        _validator = new Edit.CommandValidator();
    }

    [Fact]
    public void Should_Have_Error_When_Article_Is_Null()
    {
        // Arrange
        var command = new Edit.Command(
            Model: new Edit.Model(Article: null!),
            Slug: "test-slug"
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor("Model.Article");
    }

    [Fact]
    public void Should_Not_Have_Error_When_Article_Is_Valid()
    {
        // Arrange
        var command = new Edit.Command(
            Model: new Edit.Model(
                Article: new Edit.ArticleData(
                    Title: "Updated title",
                    Description: "Updated description",
                    Body: "Updated body",
                    TagList: ["tag1"]
                )
            ),
            Slug: "test-slug"
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Not_Have_Error_When_Article_Fields_Are_Null()
    {
        // Arrange
        // All fields can be null for edit (partial update)
        var command = new Edit.Command(
            Model: new Edit.Model(
                Article: new Edit.ArticleData(
                    Title: null,
                    Description: null,
                    Body: null,
                    TagList: null
                )
            ),
            Slug: "test-slug"
        );

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
