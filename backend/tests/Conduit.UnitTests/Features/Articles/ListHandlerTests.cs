using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Conduit.Domain;
using Conduit.Features.Articles;
using Conduit.Infrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace Conduit.UnitTests.Features.Articles;

public class ListHandlerTests : HandlerTestBase
{
    private readonly Mock<ICurrentUserAccessor> _currentUserAccessor;
    private readonly List.QueryHandler _handler;

    public ListHandlerTests()
    {
        _currentUserAccessor = new Mock<ICurrentUserAccessor>();
        _handler = new List.QueryHandler(Context, _currentUserAccessor.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnAllArticles_WhenNoFiltersApplied()
    {
        // Arrange
        var author = new Person
        {
            Username = "testuser",
            Email = "test@example.com"
        };
        Context.Persons.Add(author);

        var article1 = new Article
        {
            Title = "Article 1",
            Slug = "article-1",
            Description = "Description 1",
            Body = "Body 1",
            Author = author,
            CreatedAt = DateTime.UtcNow.AddDays(-2),
            UpdatedAt = DateTime.UtcNow.AddDays(-2)
        };

        var article2 = new Article
        {
            Title = "Article 2",
            Slug = "article-2",
            Description = "Description 2",
            Body = "Body 2",
            Author = author,
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            UpdatedAt = DateTime.UtcNow.AddDays(-1)
        };

        Context.Articles.AddRange(article1, article2);
        await Context.SaveChangesAsync();

        var query = new List.Query(
            Tag: null!,
            Author: null!,
            FavoritedUsername: null!,
            Limit: null,
            Offset: null,
            IsFeed: false
        );

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Articles.Should().HaveCount(2);
        result.ArticlesCount.Should().Be(2);
        // Should be ordered by CreatedAt descending
        result.Articles[0].Slug.Should().Be("article-2");
        result.Articles[1].Slug.Should().Be("article-1");
    }

    [Fact]
    public async Task Handle_ShouldFilterByTag_WhenTagProvided()
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

        var article1 = new Article
        {
            Title = "Article 1",
            Slug = "article-1",
            Description = "Description 1",
            Body = "Body 1",
            Author = author,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var article2 = new Article
        {
            Title = "Article 2",
            Slug = "article-2",
            Description = "Description 2",
            Body = "Body 2",
            Author = author,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        Context.Articles.AddRange(article1, article2);

        var articleTag1 = new ArticleTag
        {
            Article = article1,
            Tag = tag1
        };

        Context.ArticleTags.Add(articleTag1);
        await Context.SaveChangesAsync();

        var query = new List.Query(
            Tag: "tag1",
            Author: null!,
            FavoritedUsername: null!,
            Limit: null,
            Offset: null,
            IsFeed: false
        );

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Articles.Should().HaveCount(1);
        result.Articles[0].Slug.Should().Be("article-1");
    }

    [Fact]
    public async Task Handle_ShouldFilterByAuthor_WhenAuthorProvided()
    {
        // Arrange
        var author1 = new Person
        {
            Username = "author1",
            Email = "author1@example.com"
        };
        var author2 = new Person
        {
            Username = "author2",
            Email = "author2@example.com"
        };
        Context.Persons.AddRange(author1, author2);

        var article1 = new Article
        {
            Title = "Article 1",
            Slug = "article-1",
            Description = "Description 1",
            Body = "Body 1",
            Author = author1,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var article2 = new Article
        {
            Title = "Article 2",
            Slug = "article-2",
            Description = "Description 2",
            Body = "Body 2",
            Author = author2,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        Context.Articles.AddRange(article1, article2);
        await Context.SaveChangesAsync();

        var query = new List.Query(
            Tag: null!,
            Author: "author1",
            FavoritedUsername: null!,
            Limit: null,
            Offset: null,
            IsFeed: false
        );

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Articles.Should().HaveCount(1);
        result.Articles[0].Slug.Should().Be("article-1");
    }

    [Fact]
    public async Task Handle_ShouldFilterByFavoritedUsername_WhenProvided()
    {
        // Arrange
        var author = new Person
        {
            Username = "author",
            Email = "author@example.com"
        };
        var user = new Person
        {
            Username = "user",
            Email = "user@example.com"
        };
        Context.Persons.AddRange(author, user);

        var article1 = new Article
        {
            Title = "Article 1",
            Slug = "article-1",
            Description = "Description 1",
            Body = "Body 1",
            Author = author,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var article2 = new Article
        {
            Title = "Article 2",
            Slug = "article-2",
            Description = "Description 2",
            Body = "Body 2",
            Author = author,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        Context.Articles.AddRange(article1, article2);

        var favorite = new ArticleFavorite
        {
            Article = article1,
            Person = user
        };

        Context.ArticleFavorites.Add(favorite);
        await Context.SaveChangesAsync();

        var query = new List.Query(
            Tag: null!,
            Author: null!,
            FavoritedUsername: "user",
            Limit: null,
            Offset: null,
            IsFeed: false
        );

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Articles.Should().HaveCount(1);
        result.Articles[0].Slug.Should().Be("article-1");
    }

    [Fact]
    public async Task Handle_ShouldApplyLimitAndOffset()
    {
        // Arrange
        var author = new Person
        {
            Username = "testuser",
            Email = "test@example.com"
        };
        Context.Persons.Add(author);

        for (var i = 0; i < 10; i++)
        {
            var article = new Article
            {
                Title = $"Article {i}",
                Slug = $"article-{i}",
                Description = $"Description {i}",
                Body = $"Body {i}",
                Author = author,
                CreatedAt = DateTime.UtcNow.AddDays(-i),
                UpdatedAt = DateTime.UtcNow.AddDays(-i)
            };
            Context.Articles.Add(article);
        }

        await Context.SaveChangesAsync();

        var query = new List.Query(
            Tag: null!,
            Author: null!,
            FavoritedUsername: null!,
            Limit: 3,
            Offset: 2,
            IsFeed: false
        );

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Articles.Should().HaveCount(3);
        result.ArticlesCount.Should().Be(10);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenNonExistentTagProvided()
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
            Title = "Article",
            Slug = "article",
            Description = "Description",
            Body = "Body",
            Author = author,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        Context.Articles.Add(article);
        await Context.SaveChangesAsync();

        var query = new List.Query(
            Tag: "nonexistent",
            Author: null!,
            FavoritedUsername: null!,
            Limit: null,
            Offset: null,
            IsFeed: false
        );

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Articles.Should().BeEmpty();
    }
}
