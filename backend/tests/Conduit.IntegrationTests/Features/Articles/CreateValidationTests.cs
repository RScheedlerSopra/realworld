using System.Threading.Tasks;
using Conduit.Features.Articles;
using FluentValidation;
using Xunit;

namespace Conduit.IntegrationTests.Features.Articles;

public class CreateValidationTests : SliceFixture
{
    [Fact]
    public async Task Should_Throw_ValidationException_When_Title_Is_Empty()
    {
        // Arrange
        var command = new Create.Command(
            new Create.ArticleData
            {
                Title = "",
                Description = "Test description",
                Body = "Test body",
                IsDraft = true
            }
        );

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(async () =>
        {
            await ArticleHelpers.CreateArticle(this, command);
        });

        Assert.Contains(exception.Errors, e => e.PropertyName == "Title");
    }

    [Fact]
    public async Task Should_Throw_ValidationException_When_All_Fields_Are_Empty()
    {
        // Arrange
        var command = new Create.Command(
            new Create.ArticleData
            {
                Title = "",
                Description = "",
                Body = "",
                IsDraft = true
            }
        );

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(async () =>
        {
            await ArticleHelpers.CreateArticle(this, command);
        });

        Assert.Contains(exception.Errors, e => e.PropertyName == "Title");
        Assert.Contains(exception.Errors, e => e.PropertyName == "Description");
        Assert.Contains(exception.Errors, e => e.PropertyName == "Body");
    }

    [Fact]
    public async Task Should_Throw_ValidationException_When_Description_Is_Empty()
    {
        // Arrange
        var command = new Create.Command(
            new Create.ArticleData
            {
                Title = "Test title",
                Description = "",
                Body = "Test body",
                IsDraft = true
            }
        );

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(async () =>
        {
            await ArticleHelpers.CreateArticle(this, command);
        });

        Assert.Contains(exception.Errors, e => e.PropertyName == "Description");
    }

    [Fact]
    public async Task Should_Throw_ValidationException_When_Body_Is_Empty()
    {
        // Arrange
        var command = new Create.Command(
            new Create.ArticleData
            {
                Title = "Test title",
                Description = "Test description",
                Body = "",
                IsDraft = true
            }
        );

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(async () =>
        {
            await ArticleHelpers.CreateArticle(this, command);
        });

        Assert.Contains(exception.Errors, e => e.PropertyName == "Body");
    }
}
