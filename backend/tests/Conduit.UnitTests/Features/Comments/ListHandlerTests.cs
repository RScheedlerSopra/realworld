using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Conduit.Domain;
using Conduit.Features.Comments;
using Conduit.Infrastructure.Errors;
using FluentAssertions;
using Xunit;

namespace Conduit.UnitTests.Features.Comments;

public class ListHandlerTests : HandlerTestBase
{
    private readonly List.QueryHandler _handler;

    public ListHandlerTests()
    {
        _handler = new List.QueryHandler(Context);
    }

    [Fact]
    public async Task Handle_ShouldReturnComments_WhenArticleExists()
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

        var comment1 = new Comment
        {
            Body = "First comment",
            Author = author,
            Article = article,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        var comment2 = new Comment
        {
            Body = "Second comment",
            Author = author,
            Article = article,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        Context.Comments.AddRange(comment1, comment2);
        await Context.SaveChangesAsync();

        var query = new List.Query("test-article");

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Comments.Should().HaveCount(2);
        result.Comments.Should().Contain(c => c.Body == "First comment");
        result.Comments.Should().Contain(c => c.Body == "Second comment");
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenArticleHasNoComments()
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

        var query = new List.Query("test-article");

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Comments.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldThrowRestException_WhenArticleNotFound()
    {
        // Arrange
        var query = new List.Query("non-existent-slug");

        // Act & Assert
        var exception = await Assert.ThrowsAsync<RestException>(
            async () => await _handler.Handle(query, CancellationToken.None)
        );

        exception.Code.Should().Be(System.Net.HttpStatusCode.NotFound);
    }
}
