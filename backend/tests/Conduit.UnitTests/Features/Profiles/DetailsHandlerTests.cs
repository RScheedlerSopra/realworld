using System.Threading;
using System.Threading.Tasks;
using Conduit.Features.Profiles;
using FluentAssertions;
using Moq;
using Xunit;

namespace Conduit.UnitTests.Features.Profiles;

public class DetailsHandlerTests
{
    private readonly Mock<IProfileReader> _profileReader;
    private readonly Details.QueryHandler _handler;

    public DetailsHandlerTests()
    {
        _profileReader = new Mock<IProfileReader>();
        _handler = new Details.QueryHandler(_profileReader.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnProfile_WhenUserExists()
    {
        // Arrange
        var expectedProfile = new ProfileEnvelope(new Profile
        {
            Username = "testuser",
            Bio = "Test bio",
            Image = "http://example.com/image.jpg",
            IsFollowed = false
        });

        _profileReader.Setup(x => x.ReadProfile("testuser", CancellationToken.None))
            .ReturnsAsync(expectedProfile);

        var query = new Details.Query("testuser");

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Profile.Should().NotBeNull();
        result.Profile!.Username.Should().Be("testuser");
        result.Profile.Bio.Should().Be("Test bio");
        result.Profile.Image.Should().Be("http://example.com/image.jpg");
        result.Profile.IsFollowed.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_ShouldCallProfileReader_WithCorrectUsername()
    {
        // Arrange
        var expectedProfile = new ProfileEnvelope(new Profile
        {
            Username = "testuser",
            Bio = null,
            Image = null,
            IsFollowed = false
        });

        _profileReader.Setup(x => x.ReadProfile("testuser", CancellationToken.None))
            .ReturnsAsync(expectedProfile);

        var query = new Details.Query("testuser");

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _profileReader.Verify(x => x.ReadProfile("testuser", CancellationToken.None), Times.Once);
    }
}
