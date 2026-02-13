using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Conduit.Domain;
using Conduit.Features.Articles;
using Conduit.Infrastructure;
using Conduit.Infrastructure.Errors;
using FluentAssertions;
using Moq;
using Xunit;

namespace Conduit.UnitTests.Features.Articles;

public class ListDraftsHandlerTests : HandlerTestBase
{
    private readonly Mock<ICurrentUserAccessor> _currentUserAccessor;
    private readonly ListDrafts.QueryHandler _handler;

    public ListDraftsHandlerTests()
    {
        _currentUserAccessor = new Mock<ICurrentUserAccessor>();
        _handler = new ListDrafts.QueryHandler(Context, _currentUserAccessor.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnDrafts_WhenUserIsAuthenticated()
    {
        // Arrange
        var author = new Person
        {
            Username = "testuser",
            Email = "test@example.com"
        };
        Context.Persons.Add(author);

        var draft1 = new Article
        {
            Title = "Draft Article 1",
            Description = "Draft desc 1",
            Body = "Draft body 1",
            Slug = "draft-article-1",
            IsDraft = true,
            Author = author,
            CreatedAt = System.DateTime.UtcNow.AddDays(-2),
            UpdatedAt = System.DateTime.UtcNow.AddDays(-1)
        };

        var draft2 = new Article
        {
            Title = "Draft Article 2",
            Description = "Draft desc 2",
            Body = "Draft body 2",
            Slug = "draft-article-2",
            IsDraft = true,
            Author = author,
            CreatedAt = System.DateTime.UtcNow.AddDays(-1),
            UpdatedAt = System.DateTime.UtcNow
        };

        var publishedArticle = new Article
        {
            Title = "Published Article",
            Description = "Published desc",
            Body = "Published body",
            Slug = "published-article",
            IsDraft = false,
            Author = author,
            CreatedAt = System.DateTime.UtcNow.AddDays(-3),
            UpdatedAt = System.DateTime.UtcNow.AddDays(-3)
        };

        Context.Articles.AddRange(draft1, draft2, publishedArticle);
        await Context.SaveChangesAsync();

        _currentUserAccessor.Setup(x => x.GetCurrentUsername()).Returns("testuser");

        var query = new ListDrafts.Query(null, null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Articles.Should().HaveCount(2);
        result.ArticlesCount.Should().Be(2);
        
        // Should be ordered by UpdatedAt descending (most recent first)
        result.Articles[0].Slug.Should().Be("draft-article-2");
        result.Articles[1].Slug.Should().Be("draft-article-1");
    }

    [Fact]
    public async Task Handle_ShouldReturnOnlyOwnDrafts_WhenMultipleAuthorsExist()
    {
        // Arrange
        var author1 = new Person
        {
            Username = "testuser1",
            Email = "test1@example.com"
        };

        var author2 = new Person
        {
            Username = "testuser2",
            Email = "test2@example.com"
        };

        Context.Persons.AddRange(author1, author2);

        var draft1 = new Article
        {
            Title = "Draft by Author 1",
            Description = "Desc",
            Body = "Body",
            Slug = "draft-author-1",
            IsDraft = true,
            Author = author1,
            CreatedAt = System.DateTime.UtcNow,
            UpdatedAt = System.DateTime.UtcNow
        };

        var draft2 = new Article
        {
            Title = "Draft by Author 2",
            Description = "Desc",
            Body = "Body",
            Slug = "draft-author-2",
            IsDraft = true,
            Author = author2,
            CreatedAt = System.DateTime.UtcNow,
            UpdatedAt = System.DateTime.UtcNow
        };

        Context.Articles.AddRange(draft1, draft2);
        await Context.SaveChangesAsync();

        _currentUserAccessor.Setup(x => x.GetCurrentUsername()).Returns("testuser1");

        var query = new ListDrafts.Query(null, null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Articles.Should().HaveCount(1);
        result.Articles[0].Slug.Should().Be("draft-author-1");
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenNoDraftsExist()
    {
        // Arrange
        var author = new Person
        {
            Username = "testuser",
            Email = "test@example.com"
        };
        Context.Persons.Add(author);
        await Context.SaveChangesAsync();

        _currentUserAccessor.Setup(x => x.GetCurrentUsername()).Returns("testuser");

        var query = new ListDrafts.Query(null, null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Articles.Should().BeEmpty();
        result.ArticlesCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_ShouldRespectPagination_WhenLimitAndOffsetProvided()
    {
        // Arrange
        var author = new Person
        {
            Username = "testuser",
            Email = "test@example.com"
        };
        Context.Persons.Add(author);

        for (var i = 0; i < 5; i++)
        {
            var draft = new Article
            {
                Title = $"Draft {i}",
                Description = $"Desc {i}",
                Body = $"Body {i}",
                Slug = $"draft-{i}",
                IsDraft = true,
                Author = author,
                CreatedAt = System.DateTime.UtcNow.AddDays(-i),
                UpdatedAt = System.DateTime.UtcNow.AddDays(-i)
            };
            Context.Articles.Add(draft);
        }
        await Context.SaveChangesAsync();

        _currentUserAccessor.Setup(x => x.GetCurrentUsername()).Returns("testuser");

        var query = new ListDrafts.Query(Limit: 2, Offset: 1);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Articles.Should().HaveCount(2);
        result.ArticlesCount.Should().Be(5);
    }

    [Fact]
    public async Task Handle_ShouldThrowUnauthorized_WhenUserNotAuthenticated()
    {
        // Arrange
        _currentUserAccessor.Setup(x => x.GetCurrentUsername()).Returns((string?)null);

        var query = new ListDrafts.Query(null, null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<RestException>(
            () => _handler.Handle(query, CancellationToken.None)
        );

        exception.Code.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFound_WhenUserDoesNotExist()
    {
        // Arrange
        _currentUserAccessor.Setup(x => x.GetCurrentUsername()).Returns("nonexistentuser");

        var query = new ListDrafts.Query(null, null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<RestException>(
            () => _handler.Handle(query, CancellationToken.None)
        );

        exception.Code.Should().Be(HttpStatusCode.NotFound);
    }
}
