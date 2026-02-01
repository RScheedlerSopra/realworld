using System;
using System.Threading;
using System.Threading.Tasks;
using Conduit.Domain;
using Conduit.Features.Comments;
using Conduit.Infrastructure.Errors;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Conduit.UnitTests.Features.Comments;

public class DeleteHandlerTests : HandlerTestBase
{
    private readonly Delete.QueryHandler _handler;

    public DeleteHandlerTests()
    {
        _handler = new Delete.QueryHandler(Context);
    }

    [Fact]
    public async Task Handle_ShouldDeleteComment_WhenValidDataProvided()
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

        var comment = new Comment
        {
            Body = "Test comment",
            Author = author,
            Article = article,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        Context.Comments.Add(comment);
        await Context.SaveChangesAsync();

        var command = new Delete.Command("test-article", comment.CommentId);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        var deletedComment = await Context.Comments.FindAsync(comment.CommentId);
        deletedComment.Should().BeNull();
    }

    [Fact]
    public async Task Handle_ShouldThrowRestException_WhenArticleNotFound()
    {
        // Arrange
        var command = new Delete.Command("non-existent-slug", 1);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<RestException>(
            async () => await _handler.Handle(command, CancellationToken.None)
        );

        exception.Code.Should().Be(System.Net.HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Handle_ShouldThrowRestException_WhenCommentNotFound()
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

        var command = new Delete.Command("test-article", 999);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<RestException>(
            async () => await _handler.Handle(command, CancellationToken.None)
        );

        exception.Code.Should().Be(System.Net.HttpStatusCode.NotFound);
    }
}
