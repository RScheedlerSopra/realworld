using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Conduit.Domain;
using Conduit.Features.Favorites;
using Conduit.Infrastructure;
using Conduit.Infrastructure.Errors;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace Conduit.UnitTests.Features.Favorites;

public class AddHandlerTests : HandlerTestBase
{
    private readonly Mock<ICurrentUserAccessor> _currentUserAccessor;
    private readonly Add.QueryHandler _handler;

    public AddHandlerTests()
    {
        _currentUserAccessor = new Mock<ICurrentUserAccessor>();
        _handler = new Add.QueryHandler(Context, _currentUserAccessor.Object);
    }

    [Fact]
    public async Task Handle_ShouldAddFavorite_WhenValidDataProvided()
    {
        // Arrange
        var author = new Person
        {
            Username = "author",
            Email = "author@example.com"
        };
        var user = new Person
        {
            Username = "testuser",
            Email = "test@example.com"
        };
        Context.Persons.AddRange(author, user);

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

        var command = new Add.Command("test-article");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Article.Should().NotBeNull();

        // Verify the favorite was saved to the database
        var savedFavorites = await Context.ArticleFavorites.ToListAsync();
        savedFavorites.Should().HaveCount(1);
        savedFavorites[0].ArticleId.Should().Be(article.ArticleId);
        savedFavorites[0].PersonId.Should().Be(user.PersonId);
    }

    [Fact]
    public async Task Handle_ShouldBeIdempotent_WhenArticleAlreadyFavorited()
    {
        // Arrange
        var author = new Person
        {
            Username = "author",
            Email = "author@example.com"
        };
        var user = new Person
        {
            Username = "testuser",
            Email = "test@example.com"
        };
        Context.Persons.AddRange(author, user);

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

        var favorite = new ArticleFavorite
        {
            Article = article,
            Person = user
        };
        Context.ArticleFavorites.Add(favorite);
        await Context.SaveChangesAsync();

        _currentUserAccessor.Setup(x => x.GetCurrentUsername()).Returns("testuser");

        var command = new Add.Command("test-article");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();

        // Verify only one favorite exists
        var savedFavorites = await Context.ArticleFavorites.ToListAsync();
        savedFavorites.Should().HaveCount(1);
    }

    [Fact]
    public async Task Handle_ShouldThrowRestException_WhenArticleNotFound()
    {
        // Arrange
        var user = new Person
        {
            Username = "testuser",
            Email = "test@example.com"
        };
        Context.Persons.Add(user);
        await Context.SaveChangesAsync();

        _currentUserAccessor.Setup(x => x.GetCurrentUsername()).Returns("testuser");

        var command = new Add.Command("non-existent-slug");

        // Act & Assert
        var exception = await Assert.ThrowsAsync<RestException>(
            async () => await _handler.Handle(command, CancellationToken.None)
        );

        exception.Code.Should().Be(System.Net.HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Handle_ShouldThrowRestException_WhenUserNotFound()
    {
        // Arrange
        var author = new Person
        {
            Username = "author",
            Email = "author@example.com"
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

        _currentUserAccessor.Setup(x => x.GetCurrentUsername()).Returns("non-existent-user");

        var command = new Add.Command("test-article");

        // Act & Assert
        var exception = await Assert.ThrowsAsync<RestException>(
            async () => await _handler.Handle(command, CancellationToken.None)
        );

        exception.Code.Should().Be(System.Net.HttpStatusCode.NotFound);
    }
}
