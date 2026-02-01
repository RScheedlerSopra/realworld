using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Conduit.Domain;
using Conduit.Features.Articles;
using Conduit.Infrastructure;
using FluentAssertions;
using Moq;
using Xunit;

namespace Conduit.UnitTests.Features.Articles;

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
    public async Task Handle_ShouldCreateArticle_WhenValidDataProvided()
    {
        // Arrange
        var author = new Person
        {
            Username = "testuser",
            Email = "test@example.com",
            Bio = "Test bio",
            Image = "http://example.com/image.jpg"
        };
        Context.Persons.Add(author);
        await Context.SaveChangesAsync();

        _currentUserAccessor.Setup(x => x.GetCurrentUsername()).Returns("testuser");

        var command = new Create.Command(
            Article: new Create.ArticleData
            {
                Title = "Test Article",
                Description = "Test Description",
                Body = "Test Body",
                TagList = ["test", "article"]
            }
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Article.Should().NotBeNull();
        result.Article!.Title.Should().Be("Test Article");
        result.Article.Description.Should().Be("Test Description");
        result.Article.Body.Should().Be("Test Body");
        result.Article.Slug.Should().Be("test-article");
        result.Article.TagList.Should().HaveCount(2);
        result.Article.TagList.Should().Contain("test");
        result.Article.TagList.Should().Contain("article");

        // Verify the article was saved to the database
        var savedArticle = await Context.Articles.FindAsync(result.Article.ArticleId);
        savedArticle.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_ShouldCreateArticle_WithoutTags()
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

        var command = new Create.Command(
            Article: new Create.ArticleData
            {
                Title = "Test Article Without Tags",
                Description = "Test Description",
                Body = "Test Body",
                TagList = null
            }
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Article.Should().NotBeNull();
        result.Article!.TagList.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldReuseExistingTags()
    {
        // Arrange
        var author = new Person
        {
            Username = "testuser",
            Email = "test@example.com"
        };
        Context.Persons.Add(author);

        var existingTag = new Tag { TagId = "existing" };
        Context.Tags.Add(existingTag);
        await Context.SaveChangesAsync();

        _currentUserAccessor.Setup(x => x.GetCurrentUsername()).Returns("testuser");

        var command = new Create.Command(
            Article: new Create.ArticleData
            {
                Title = "Test Article",
                Description = "Test Description",
                Body = "Test Body",
                TagList = ["existing", "new"]
            }
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Article.Should().NotBeNull();
        result.Article!.TagList.Should().HaveCount(2);

        // Verify only one "existing" tag exists in database
        var tagsInDb = Context.Tags.Where(t => t.TagId == "existing").ToList();
        tagsInDb.Should().HaveCount(1);
    }

    [Fact]
    public async Task Handle_ShouldGenerateSlugFromTitle()
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

        var command = new Create.Command(
            Article: new Create.ArticleData
            {
                Title = "This is a Test Article Title",
                Description = "Test Description",
                Body = "Test Body"
            }
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Article.Should().NotBeNull();
        result.Article!.Slug.Should().Be("this-is-a-test-article-title");
    }
}
