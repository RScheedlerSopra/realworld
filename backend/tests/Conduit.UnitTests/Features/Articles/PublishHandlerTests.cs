using System;
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

public class PublishHandlerTests : HandlerTestBase
{
    private readonly Mock<ICurrentUserAccessor> _currentUserAccessor;
    private readonly Publish.Handler _handler;

    public PublishHandlerTests()
    {
        _currentUserAccessor = new Mock<ICurrentUserAccessor>();
        _handler = new Publish.Handler(Context, _currentUserAccessor.Object);
    }

    [Fact]
    public async Task Handle_ShouldPublishDraft_WhenValidDraftAndAuthorizedUser()
    {
        // Arrange
        var author = new Person
        {
            Username = "testuser",
            Email = "test@example.com"
        };
        Context.Persons.Add(author);

        var draft = new Article
        {
            Title = "Draft Article",
            Description = "Draft description",
            Body = "Draft body",
            Slug = "draft-article",
            IsDraft = true,
            Author = author,
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            UpdatedAt = DateTime.UtcNow.AddDays(-1)
        };
        Context.Articles.Add(draft);
        await Context.SaveChangesAsync();

        _currentUserAccessor.Setup(x => x.GetCurrentUsername()).Returns("testuser");

        var command = new Publish.Command("draft-article");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Article.Should().NotBeNull();
        result.Article!.IsDraft.Should().BeFalse();
        result.Article.Slug.Should().Be("draft-article");
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFound_WhenArticleDoesNotExist()
    {
        // Arrange
        _currentUserAccessor.Setup(x => x.GetCurrentUsername()).Returns("testuser");

        var command = new Publish.Command("non-existent-slug");

        // Act & Assert
        var exception = await Assert.ThrowsAsync<RestException>(
            () => _handler.Handle(command, CancellationToken.None)
        );

        exception.Code.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Handle_ShouldThrowBadRequest_WhenArticleAlreadyPublished()
    {
        // Arrange
        var author = new Person
        {
            Username = "testuser",
            Email = "test@example.com"
        };
        Context.Persons.Add(author);

        var published = new Article
        {
            Title = "Published Article",
            Description = "Published description",
            Body = "Published body",
            Slug = "published-article",
            IsDraft = false,
            Author = author,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        Context.Articles.Add(published);
        await Context.SaveChangesAsync();

        _currentUserAccessor.Setup(x => x.GetCurrentUsername()).Returns("testuser");

        var command = new Publish.Command("published-article");

        // Act & Assert
        var exception = await Assert.ThrowsAsync<RestException>(
            () => _handler.Handle(command, CancellationToken.None)
        );

        exception.Code.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Handle_ShouldThrowForbidden_WhenUserNotAuthor()
    {
        // Arrange
        var author = new Person
        {
            Username = "author",
            Email = "author@example.com"
        };
        Context.Persons.Add(author);

        var draft = new Article
        {
            Title = "Draft Article",
            Description = "Draft description",
            Body = "Draft body",
            Slug = "draft-article",
            IsDraft = true,
            Author = author,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        Context.Articles.Add(draft);
        await Context.SaveChangesAsync();

        _currentUserAccessor.Setup(x => x.GetCurrentUsername()).Returns("otheruser");

        var command = new Publish.Command("draft-article");

        // Act & Assert
        var exception = await Assert.ThrowsAsync<RestException>(
            () => _handler.Handle(command, CancellationToken.None)
        );

        exception.Code.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Handle_ShouldHandleSlugCollision_WhenPublishing()
    {
        // Arrange
        var author = new Person
        {
            Username = "testuser",
            Email = "test@example.com"
        };
        Context.Persons.Add(author);

        // Create an existing published article with the same slug
        var existingArticle = new Article
        {
            Title = "Existing Article",
            Description = "Existing description",
            Body = "Existing body",
            Slug = "same-slug",
            IsDraft = false,
            Author = author,
            CreatedAt = DateTime.UtcNow.AddDays(-2),
            UpdatedAt = DateTime.UtcNow.AddDays(-2)
        };

        // Create a draft with the same slug
        var draft = new Article
        {
            Title = "Same Slug",
            Description = "Draft description",
            Body = "Draft body",
            Slug = "same-slug",
            IsDraft = true,
            Author = author,
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            UpdatedAt = DateTime.UtcNow.AddDays(-1)
        };

        Context.Articles.AddRange(existingArticle, draft);
        await Context.SaveChangesAsync();

        _currentUserAccessor.Setup(x => x.GetCurrentUsername()).Returns("testuser");

        var command = new Publish.Command("same-slug");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Article.Should().NotBeNull();
        result.Article!.IsDraft.Should().BeFalse();
        result.Article.Slug.Should().NotBe("same-slug");
        result.Article.Slug.Should().StartWith("same-slug-");
    }

    [Fact]
    public async Task Handle_ShouldHandleMultipleSlugCollisions()
    {
        // Arrange
        var author = new Person
        {
            Username = "testuser",
            Email = "test@example.com"
        };
        Context.Persons.Add(author);

        // Create multiple published articles with similar slugs
        var article1 = new Article
        {
            Title = "Collision",
            Description = "Desc",
            Body = "Body",
            Slug = "collision-slug",
            IsDraft = false,
            Author = author,
            CreatedAt = DateTime.UtcNow.AddDays(-3),
            UpdatedAt = DateTime.UtcNow.AddDays(-3)
        };

        var article2 = new Article
        {
            Title = "Collision",
            Description = "Desc",
            Body = "Body",
            Slug = "collision-slug-1",
            IsDraft = false,
            Author = author,
            CreatedAt = DateTime.UtcNow.AddDays(-2),
            UpdatedAt = DateTime.UtcNow.AddDays(-2)
        };

        // Create a draft with the same slug
        var draft = new Article
        {
            Title = "Collision",
            Description = "Draft description",
            Body = "Draft body",
            Slug = "collision-slug",
            IsDraft = true,
            Author = author,
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            UpdatedAt = DateTime.UtcNow.AddDays(-1)
        };

        Context.Articles.AddRange(article1, article2, draft);
        await Context.SaveChangesAsync();

        _currentUserAccessor.Setup(x => x.GetCurrentUsername()).Returns("testuser");

        var command = new Publish.Command("collision-slug");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Article.Should().NotBeNull();
        result.Article!.IsDraft.Should().BeFalse();
        result.Article.Slug.Should().Be("collision-slug-2");
    }

    [Fact]
    public async Task Handle_ShouldUpdateTimestamp_WhenPublishing()
    {
        // Arrange
        var author = new Person
        {
            Username = "testuser",
            Email = "test@example.com"
        };
        Context.Persons.Add(author);

        var oldUpdateTime = DateTime.UtcNow.AddDays(-1);
        var draft = new Article
        {
            Title = "Draft Article",
            Description = "Draft description",
            Body = "Draft body",
            Slug = "draft-article",
            IsDraft = true,
            Author = author,
            CreatedAt = DateTime.UtcNow.AddDays(-2),
            UpdatedAt = oldUpdateTime
        };
        Context.Articles.Add(draft);
        await Context.SaveChangesAsync();

        _currentUserAccessor.Setup(x => x.GetCurrentUsername()).Returns("testuser");

        var command = new Publish.Command("draft-article");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Article.Should().NotBeNull();
        result.Article!.UpdatedAt.Should().BeAfter(oldUpdateTime);
    }
}
