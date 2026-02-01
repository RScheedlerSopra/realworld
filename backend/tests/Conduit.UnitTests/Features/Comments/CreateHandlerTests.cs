using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Conduit.Domain;
using Conduit.Features.Comments;
using Conduit.Infrastructure;
using Conduit.Infrastructure.Errors;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace Conduit.UnitTests.Features.Comments;

public class CreateHandlerTests : HandlerTestBase
{
    private readonly Mock<ICurrentUserAccessor> _currentUserAccessor;
    private readonly Create.Handler _handler;

    public CreateHandlerTests()
    {
        _currentUserAccessor = new Mock<ICurrentUserAccessor>();
        _handler = new Create.Handler(Context, _currentUserAccessor.Object);
    }

    [Fact]
    public async Task Handle_ShouldCreateComment_WhenValidDataProvided()
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

        _currentUserAccessor.Setup(x => x.GetCurrentUsername()).Returns("testuser");

        var model = new Create.Model(new Create.CommentData(Body: "This is a test comment"));
        var command = new Create.Command(model, "test-article");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Comment.Should().NotBeNull();
        result.Comment!.Body.Should().Be("This is a test comment");
        result.Comment.Author.Should().NotBeNull();
        result.Comment.Author!.Username.Should().Be("testuser");

        // Verify the comment was saved to the database
        var savedComments = await Context.Comments.ToListAsync();
        savedComments.Should().HaveCount(1);
        savedComments[0].Body.Should().Be("This is a test comment");
    }

    [Fact]
    public async Task Handle_ShouldThrowRestException_WhenArticleNotFound()
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

        var model = new Create.Model(new Create.CommentData(Body: "This is a test comment"));
        var command = new Create.Command(model, "non-existent-slug");

        // Act & Assert
        var exception = await Assert.ThrowsAsync<RestException>(
            async () => await _handler.Handle(command, CancellationToken.None)
        );

        exception.Code.Should().Be(System.Net.HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Handle_ShouldSetCreatedAndUpdatedTimestamps()
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

        _currentUserAccessor.Setup(x => x.GetCurrentUsername()).Returns("testuser");

        var model = new Create.Model(new Create.CommentData(Body: "This is a test comment"));
        var command = new Create.Command(model, "test-article");

        var beforeTime = DateTime.UtcNow;

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        var afterTime = DateTime.UtcNow;

        // Assert
        result.Comment!.CreatedAt.Should().BeOnOrAfter(beforeTime);
        result.Comment.CreatedAt.Should().BeOnOrBefore(afterTime);
        result.Comment.UpdatedAt.Should().BeOnOrAfter(beforeTime);
        result.Comment.UpdatedAt.Should().BeOnOrBefore(afterTime);
    }
}
