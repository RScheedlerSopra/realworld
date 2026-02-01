using Conduit.Features.Articles;
using FluentValidation.TestHelper;
using Xunit;

namespace Conduit.UnitTests.Features.Articles;

public class CreateArticleDataValidatorTests
{
    private readonly Create.ArticleDataValidator _validator;

    public CreateArticleDataValidatorTests()
    {
        _validator = new Create.ArticleDataValidator();
    }

    [Fact]
    public void Should_Have_Error_When_Title_Is_Null()
    {
        // Arrange
        var model = new Create.ArticleData
        {
            Title = null,
            Description = "Test description",
            Body = "Test body"
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Title);
    }

    [Fact]
    public void Should_Have_Error_When_Title_Is_Empty()
    {
        // Arrange
        var model = new Create.ArticleData
        {
            Title = "",
            Description = "Test description",
            Body = "Test body"
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Title);
    }

    [Fact]
    public void Should_Have_Error_When_Description_Is_Null()
    {
        // Arrange
        var model = new Create.ArticleData
        {
            Title = "Test title",
            Description = null,
            Body = "Test body"
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void Should_Have_Error_When_Description_Is_Empty()
    {
        // Arrange
        var model = new Create.ArticleData
        {
            Title = "Test title",
            Description = "",
            Body = "Test body"
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void Should_Have_Error_When_Body_Is_Null()
    {
        // Arrange
        var model = new Create.ArticleData
        {
            Title = "Test title",
            Description = "Test description",
            Body = null
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Body);
    }

    [Fact]
    public void Should_Have_Error_When_Body_Is_Empty()
    {
        // Arrange
        var model = new Create.ArticleData
        {
            Title = "Test title",
            Description = "Test description",
            Body = ""
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Body);
    }

    [Fact]
    public void Should_Not_Have_Error_When_All_Required_Fields_Are_Valid()
    {
        // Arrange
        var model = new Create.ArticleData
        {
            Title = "Test title",
            Description = "Test description",
            Body = "Test body"
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Title);
        result.ShouldNotHaveValidationErrorFor(x => x.Description);
        result.ShouldNotHaveValidationErrorFor(x => x.Body);
    }

    [Fact]
    public void Should_Not_Have_Error_When_TagList_Is_Null()
    {
        // Arrange
        var model = new Create.ArticleData
        {
            Title = "Test title",
            Description = "Test description",
            Body = "Test body",
            TagList = null
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Not_Have_Error_When_TagList_Is_Empty()
    {
        // Arrange
        var model = new Create.ArticleData
        {
            Title = "Test title",
            Description = "Test description",
            Body = "Test body",
            TagList = []
        };

        // Act
        var result = _validator.TestValidate(model);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
