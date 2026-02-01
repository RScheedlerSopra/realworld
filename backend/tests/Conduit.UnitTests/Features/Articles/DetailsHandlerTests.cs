using System;
using System.Threading;
using System.Threading.Tasks;
using Conduit.Domain;
using Conduit.Features.Articles;
using Conduit.Infrastructure.Errors;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Conduit.UnitTests.Features.Articles;

public class DetailsHandlerTests : HandlerTestBase
{
    private readonly Details.QueryHandler _handler;

    public DetailsHandlerTests()
    {
        _handler = new Details.QueryHandler(Context);
    }

    [Fact]
    public async Task Handle_ShouldReturnArticle_WhenArticleExists()
    {
        // Arrange
        var author = new Person
        {
            Username = "testuser",
            Email = "test@example.com",
            Bio = "Test bio"
        };

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

        Context.Persons.Add(author);
        Context.Articles.Add(article);
        await Context.SaveChangesAsync();

        var query = new Details.Query(Slug: "test-article");

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Article.Should().NotBeNull();
        result.Article!.Slug.Should().Be("test-article");
        result.Article.Title.Should().Be("Test Article");
        result.Article.Description.Should().Be("Test Description");
        result.Article.Body.Should().Be("Test Body");
        result.Article.Author.Should().NotBeNull();
        result.Article.Author!.Username.Should().Be("testuser");
    }

    [Fact]
    public async Task Handle_ShouldThrowRestException_WhenArticleDoesNotExist()
    {
        // Arrange
        var query = new Details.Query(Slug: "non-existent-article");

        // Act
        Func<Task> act = async () => await _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<RestException>()
            .Where(e => e.Code == System.Net.HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Handle_ShouldReturnArticleWithTags()
    {
        // Arrange
        var author = new Person
        {
            Username = "testuser",
            Email = "test@example.com"
        };

        var tag1 = new Tag { TagId = "tag1" };
        var tag2 = new Tag { TagId = "tag2" };

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

        Context.Persons.Add(author);
        Context.Tags.AddRange(tag1, tag2);
        Context.Articles.Add(article);
        await Context.SaveChangesAsync();

        // Add article tags
        Context.ArticleTags.Add(new ArticleTag
        {
            ArticleId = article.ArticleId,
            Article = article,
            TagId = "tag1",
            Tag = tag1
        });
        Context.ArticleTags.Add(new ArticleTag
        {
            ArticleId = article.ArticleId,
            Article = article,
            TagId = "tag2",
            Tag = tag2
        });
        await Context.SaveChangesAsync();

        var query = new Details.Query(Slug: "test-article");

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Article.Should().NotBeNull();
        result.Article!.TagList.Should().HaveCount(2);
        result.Article.TagList.Should().Contain("tag1");
        result.Article.TagList.Should().Contain("tag2");
    }

    [Fact]
    public async Task Handle_ShouldReturnArticleWithFavoriteCount()
    {
        // Arrange
        var author = new Person
        {
            Username = "author",
            Email = "author@example.com"
        };

        var user1 = new Person
        {
            Username = "user1",
            Email = "user1@example.com"
        };

        var user2 = new Person
        {
            Username = "user2",
            Email = "user2@example.com"
        };

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

        Context.Persons.AddRange(author, user1, user2);
        Context.Articles.Add(article);
        await Context.SaveChangesAsync();

        // Add favorites
        Context.ArticleFavorites.Add(new ArticleFavorite
        {
            ArticleId = article.ArticleId,
            Article = article,
            PersonId = user1.PersonId,
            Person = user1
        });
        Context.ArticleFavorites.Add(new ArticleFavorite
        {
            ArticleId = article.ArticleId,
            Article = article,
            PersonId = user2.PersonId,
            Person = user2
        });
        await Context.SaveChangesAsync();

        var query = new Details.Query(Slug: "test-article");

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Article.Should().NotBeNull();
        result.Article!.FavoritesCount.Should().Be(2);
    }
}
