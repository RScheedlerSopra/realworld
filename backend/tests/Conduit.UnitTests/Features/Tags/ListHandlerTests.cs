using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Conduit.Domain;
using Conduit.Features.Tags;
using FluentAssertions;
using Xunit;

namespace Conduit.UnitTests.Features.Tags;

public class ListHandlerTests : HandlerTestBase
{
    private readonly List.QueryHandler _handler;

    public ListHandlerTests()
    {
        _handler = new List.QueryHandler(Context);
    }

    [Fact]
    public async Task Handle_ShouldReturnAllTags_WhenTagsExist()
    {
        // Arrange
        var tag1 = new Tag { TagId = "backend" };
        var tag2 = new Tag { TagId = "frontend" };
        var tag3 = new Tag { TagId = "test" };
        Context.Tags.AddRange(tag1, tag2, tag3);
        await Context.SaveChangesAsync();

        var query = new List.Query();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Tags.Should().HaveCount(3);
        result.Tags.Should().Contain("backend");
        result.Tags.Should().Contain("frontend");
        result.Tags.Should().Contain("test");
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenNoTagsExist()
    {
        // Arrange
        var query = new List.Query();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Tags.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldReturnTagsInAlphabeticalOrder()
    {
        // Arrange
        var tag1 = new Tag { TagId = "zebra" };
        var tag2 = new Tag { TagId = "alpha" };
        var tag3 = new Tag { TagId = "beta" };
        Context.Tags.AddRange(tag1, tag2, tag3);
        await Context.SaveChangesAsync();

        var query = new List.Query();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Tags.Should().HaveCount(3);
        result.Tags[0].Should().Be("alpha");
        result.Tags[1].Should().Be("beta");
        result.Tags[2].Should().Be("zebra");
    }
}
