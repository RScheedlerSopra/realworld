using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Conduit.Domain;
using Conduit.Features.Articles;
using Conduit.Infrastructure.Errors;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Conduit.UnitTests.Features.Articles;

public class EditHandlerTests : HandlerTestBase
{
    private readonly Edit.Handler _handler;

    public EditHandlerTests()
    {
        _handler = new Edit.Handler(Context);
    }

    [Fact]
    public async Task Handle_ShouldUpdateArticle_WhenValidDataProvided()
    {
        // Arrange
        var author = new Person
        {
            Username = "testuser",
            Email = "test@example.com"
        };
        Context.Persons.Add(author);

        var article = new Article
        {
            Title = "Original Title",
            Slug = "original-title",
            Description = "Original Description",
            Body = "Original Body",
            Author = author,
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            UpdatedAt = DateTime.UtcNow.AddDays(-1)
        };
        Context.Articles.Add(article);
        await Context.SaveChangesAsync();

        var model = new Edit.Model(
            new Edit.ArticleData(
                Title: "Updated Title",
                Description: "Updated Description",
                Body: "Updated Body",
                TagList: null
            )
        );
        var command = new Edit.Command(model, "original-title");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Article.Should().NotBeNull();
        result.Article!.Title.Should().Be("Updated Title");
        result.Article.Description.Should().Be("Updated Description");
        result.Article.Body.Should().Be("Updated Body");
        result.Article.Slug.Should().Be("updated-title");

        // Verify UpdatedAt was changed
        var updatedArticle = await Context.Articles.FirstOrDefaultAsync(x => x.Slug == "updated-title");
        updatedArticle!.UpdatedAt.Should().BeAfter(updatedArticle.CreatedAt);
    }

    [Fact]
    public async Task Handle_ShouldUpdateOnlyProvidedFields()
    {
        // Arrange
        var author = new Person
        {
            Username = "testuser",
            Email = "test@example.com"
        };
        Context.Persons.Add(author);

        var article = new Article
        {
            Title = "Original Title",
            Slug = "original-title",
            Description = "Original Description",
            Body = "Original Body",
            Author = author,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        Context.Articles.Add(article);
        await Context.SaveChangesAsync();

        var model = new Edit.Model(
            new Edit.ArticleData(
                Title: null,
                Description: "Updated Description",
                Body: null,
                TagList: null
            )
        );
        var command = new Edit.Command(model, "original-title");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Article.Should().NotBeNull();
        result.Article!.Title.Should().Be("Original Title");
        result.Article.Description.Should().Be("Updated Description");
        result.Article.Body.Should().Be("Original Body");
    }

    [Fact]
    public async Task Handle_ShouldAddTags_WhenTagListProvided()
    {
        // Arrange
        var author = new Person
        {
            Username = "testuser",
            Email = "test@example.com"
        };
        Context.Persons.Add(author);

        var article = new Article
        {
            Title = "Test Article",
            Slug = "test-article",
            Description = "Test Description",
            Body = "Test Body",
            Author = author,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        Context.Articles.Add(article);
        await Context.SaveChangesAsync();

        var model = new Edit.Model(
            new Edit.ArticleData(
                Title: null,
                Description: null,
                Body: null,
                TagList: ["tag1", "tag2"]
            )
        );
        var command = new Edit.Command(model, "test-article");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Article.Should().NotBeNull();
        result.Article!.TagList.Should().HaveCount(2);
        result.Article.TagList.Should().Contain("tag1");
        result.Article.TagList.Should().Contain("tag2");
    }

    [Fact]
    public async Task Handle_ShouldRemoveTags_WhenTagsNotInNewList()
    {
        // Arrange
        var author = new Person
        {
            Username = "testuser",
            Email = "test@example.com"
        };
        Context.Persons.Add(author);

        var tag1 = new Tag { TagId = "tag1" };
        var tag2 = new Tag { TagId = "tag2" };
        Context.Tags.AddRange(tag1, tag2);

        var article = new Article
        {
            Title = "Test Article",
            Slug = "test-article",
            Description = "Test Description",
            Body = "Test Body",
            Author = author,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        Context.Articles.Add(article);

        var articleTag1 = new ArticleTag
        {
            Article = article,
            Tag = tag1
        };
        var articleTag2 = new ArticleTag
        {
            Article = article,
            Tag = tag2
        };
        Context.ArticleTags.AddRange(articleTag1, articleTag2);
        await Context.SaveChangesAsync();

        var model = new Edit.Model(
            new Edit.ArticleData(
                Title: null,
                Description: null,
                Body: null,
                TagList: ["tag1"] // Only keep tag1
            )
        );
        var command = new Edit.Command(model, "test-article");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Article.Should().NotBeNull();
        result.Article!.TagList.Should().HaveCount(1);
        result.Article.TagList.Should().Contain("tag1");
        result.Article.TagList.Should().NotContain("tag2");
    }

    [Fact]
    public async Task Handle_ShouldThrowRestException_WhenArticleNotFound()
    {
        // Arrange
        var model = new Edit.Model(
            new Edit.ArticleData(
                Title: "Updated Title",
                Description: "Updated Description",
                Body: "Updated Body",
                TagList: null
            )
        );
        var command = new Edit.Command(model, "non-existent-slug");

        // Act & Assert
        var exception = await Assert.ThrowsAsync<RestException>(
            async () => await _handler.Handle(command, CancellationToken.None)
        );

        exception.Code.Should().Be(System.Net.HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Handle_ShouldUpdateSlug_WhenTitleChanges()
    {
        // Arrange
        var author = new Person
        {
            Username = "testuser",
            Email = "test@example.com"
        };
        Context.Persons.Add(author);

        var article = new Article
        {
            Title = "Original Title",
            Slug = "original-title",
            Description = "Test Description",
            Body = "Test Body",
            Author = author,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        Context.Articles.Add(article);
        await Context.SaveChangesAsync();

        var model = new Edit.Model(
            new Edit.ArticleData(
                Title: "New Amazing Title",
                Description: null,
                Body: null,
                TagList: null
            )
        );
        var command = new Edit.Command(model, "original-title");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Article!.Slug.Should().Be("new-amazing-title");
    }
}
