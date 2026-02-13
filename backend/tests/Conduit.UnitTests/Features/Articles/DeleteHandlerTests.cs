using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Conduit.Domain;
using Conduit.Features.Articles;
using Conduit.Infrastructure;
using Conduit.Infrastructure.Errors;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace Conduit.UnitTests.Features.Articles;

public class DeleteHandlerTests : HandlerTestBase
{
    private readonly Mock<ICurrentUserAccessor> _currentUserAccessor;
    private readonly Delete.QueryHandler _handler;

    public DeleteHandlerTests()
    {
        _currentUserAccessor = new Mock<ICurrentUserAccessor>();
        _handler = new Delete.QueryHandler(Context, _currentUserAccessor.Object);
    }

    [Fact]
    public async Task Handle_ShouldDeleteArticle_WhenArticleExists()
    {
        // Arrange
        var author = new Person
        {
            Username = "testuser",
            Email = "test@example.com"
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

        _currentUserAccessor.Setup(x => x.GetCurrentUsername()).Returns("testuser");

        var articleId = article.ArticleId;
        var command = new Delete.Command(Slug: "test-article");

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        var deletedArticle = await Context.Articles.FindAsync(articleId);
        deletedArticle.Should().BeNull();
    }

    [Fact]
    public async Task Handle_ShouldThrowRestException_WhenArticleDoesNotExist()
    {
        // Arrange
        _currentUserAccessor.Setup(x => x.GetCurrentUsername()).Returns("testuser");
        var command = new Delete.Command(Slug: "non-existent-article");

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<RestException>()
            .Where(e => e.Code == System.Net.HttpStatusCode.NotFound);
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

        _currentUserAccessor.Setup(x => x.GetCurrentUsername()).Returns("otheruser");

        var command = new Delete.Command(Slug: "test-article");

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<RestException>()
            .Where(e => e.Code == HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Handle_ShouldDeleteDraft_WhenAuthorDeletes()
    {
        // Arrange
        var author = new Person
        {
            Username = "testuser",
            Email = "test@example.com"
        };

        var draft = new Article
        {
            Title = "Draft Article",
            Slug = "draft-article",
            Description = "Draft Description",
            Body = "Draft Body",
            IsDraft = true,
            Author = author,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        Context.Persons.Add(author);
        Context.Articles.Add(draft);
        await Context.SaveChangesAsync();

        _currentUserAccessor.Setup(x => x.GetCurrentUsername()).Returns("testuser");

        var articleId = draft.ArticleId;
        var command = new Delete.Command(Slug: "draft-article");

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        var deletedArticle = await Context.Articles.FindAsync(articleId);
        deletedArticle.Should().BeNull();
    }

    [Fact]
    public async Task Handle_ShouldDeleteArticleAndCascadeToTags()
    {
        // Arrange
        var author = new Person
        {
            Username = "testuser",
            Email = "test@example.com"
        };

        var tag = new Tag { TagId = "test-tag" };

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
        Context.Tags.Add(tag);
        Context.Articles.Add(article);
        await Context.SaveChangesAsync();

        Context.ArticleTags.Add(new ArticleTag
        {
            ArticleId = article.ArticleId,
            Article = article,
            TagId = "test-tag",
            Tag = tag
        });
        await Context.SaveChangesAsync();

        _currentUserAccessor.Setup(x => x.GetCurrentUsername()).Returns("testuser");

        var articleId = article.ArticleId;
        var command = new Delete.Command(Slug: "test-article");

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        var deletedArticle = await Context.Articles.FindAsync(articleId);
        deletedArticle.Should().BeNull();

        // ArticleTags relationship should be removed (cascade delete)
        var articleTags = await Context.ArticleTags
            .Where(at => at.ArticleId == articleId)
            .ToListAsync();
        articleTags.Should().BeEmpty();

        // Tag itself should still exist
        var tagStillExists = await Context.Tags.FindAsync("test-tag");
        tagStillExists.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_ShouldDeleteArticleAndCascadeToFavorites()
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

        Context.Persons.AddRange(author, user);
        Context.Articles.Add(article);
        await Context.SaveChangesAsync();

        Context.ArticleFavorites.Add(new ArticleFavorite
        {
            ArticleId = article.ArticleId,
            Article = article,
            PersonId = user.PersonId,
            Person = user
        });
        await Context.SaveChangesAsync();

        _currentUserAccessor.Setup(x => x.GetCurrentUsername()).Returns("author");

        var articleId = article.ArticleId;
        var command = new Delete.Command(Slug: "test-article");

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        var deletedArticle = await Context.Articles.FindAsync(articleId);
        deletedArticle.Should().BeNull();

        // ArticleFavorites relationship should be removed (cascade delete)
        var articleFavorites = await Context.ArticleFavorites
            .Where(af => af.ArticleId == articleId)
            .ToListAsync();
        articleFavorites.Should().BeEmpty();

        // User should still exist
        var userStillExists = await Context.Persons.FindAsync(user.PersonId);
        userStillExists.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_ShouldDeleteArticleAndCascadeToComments()
    {
        // Arrange
        var author = new Person
        {
            Username = "author",
            Email = "author@example.com"
        };

        var commenter = new Person
        {
            Username = "commenter",
            Email = "commenter@example.com"
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

        Context.Persons.AddRange(author, commenter);
        Context.Articles.Add(article);
        await Context.SaveChangesAsync();

        var comment = new Comment
        {
            Body = "Test comment",
            Author = commenter,
            Article = article,
            CreatedAt = DateTime.UtcNow
        };
        Context.Comments.Add(comment);
        await Context.SaveChangesAsync();

        _currentUserAccessor.Setup(x => x.GetCurrentUsername()).Returns("author");

        var articleId = article.ArticleId;
        var commentId = comment.CommentId;
        var command = new Delete.Command(Slug: "test-article");

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        var deletedArticle = await Context.Articles.FindAsync(articleId);
        deletedArticle.Should().BeNull();

        // Comment should be removed (cascade delete)
        var deletedComment = await Context.Comments.FindAsync(commentId);
        deletedComment.Should().BeNull();

        // Commenter should still exist
        var commenterStillExists = await Context.Persons.FindAsync(commenter.PersonId);
        commenterStillExists.Should().NotBeNull();
    }
}
