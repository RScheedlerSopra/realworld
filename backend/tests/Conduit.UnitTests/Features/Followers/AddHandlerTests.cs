using System.Threading;
using System.Threading.Tasks;
using Conduit.Domain;
using Conduit.Features.Followers;
using Conduit.Features.Profiles;
using Conduit.Infrastructure;
using Conduit.Infrastructure.Errors;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace Conduit.UnitTests.Features.Followers;

public class AddHandlerTests : HandlerTestBase
{
    private readonly Mock<ICurrentUserAccessor> _currentUserAccessor;
    private readonly Mock<IProfileReader> _profileReader;
    private readonly Add.QueryHandler _handler;

    public AddHandlerTests()
    {
        _currentUserAccessor = new Mock<ICurrentUserAccessor>();
        _profileReader = new Mock<IProfileReader>();
        _handler = new Add.QueryHandler(Context, _currentUserAccessor.Object, _profileReader.Object);
    }

    [Fact]
    public async Task Handle_ShouldAddFollower_WhenValidDataProvided()
    {
        // Arrange
        var target = new Person
        {
            Username = "targetuser",
            Email = "target@example.com"
        };
        var observer = new Person
        {
            Username = "observeruser",
            Email = "observer@example.com"
        };
        Context.Persons.AddRange(target, observer);
        await Context.SaveChangesAsync();

        _currentUserAccessor.Setup(x => x.GetCurrentUsername()).Returns("observeruser");
        _profileReader.Setup(x => x.ReadProfile("targetuser", CancellationToken.None))
            .ReturnsAsync(new ProfileEnvelope(new Profile
            {
                Username = "targetuser",
                Bio = null,
                Image = null,
                IsFollowed = true
            }));

        var command = new Add.Command("targetuser");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();

        // Verify the follow relationship was saved
        var savedFollows = await Context.FollowedPeople.ToListAsync();
        savedFollows.Should().HaveCount(1);
        savedFollows[0].ObserverId.Should().Be(observer.PersonId);
        savedFollows[0].TargetId.Should().Be(target.PersonId);
    }

    [Fact]
    public async Task Handle_ShouldBeIdempotent_WhenAlreadyFollowing()
    {
        // Arrange
        var target = new Person
        {
            Username = "targetuser",
            Email = "target@example.com"
        };
        var observer = new Person
        {
            Username = "observeruser",
            Email = "observer@example.com"
        };
        Context.Persons.AddRange(target, observer);

        var followedPeople = new FollowedPeople
        {
            Observer = observer,
            Target = target
        };
        Context.FollowedPeople.Add(followedPeople);
        await Context.SaveChangesAsync();

        _currentUserAccessor.Setup(x => x.GetCurrentUsername()).Returns("observeruser");
        _profileReader.Setup(x => x.ReadProfile("targetuser", CancellationToken.None))
            .ReturnsAsync(new ProfileEnvelope(new Profile
            {
                Username = "targetuser",
                Bio = null,
                Image = null,
                IsFollowed = true
            }));

        var command = new Add.Command("targetuser");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();

        // Verify only one follow relationship exists
        var savedFollows = await Context.FollowedPeople.ToListAsync();
        savedFollows.Should().HaveCount(1);
    }

    [Fact]
    public async Task Handle_ShouldThrowRestException_WhenTargetUserNotFound()
    {
        // Arrange
        var observer = new Person
        {
            Username = "observeruser",
            Email = "observer@example.com"
        };
        Context.Persons.Add(observer);
        await Context.SaveChangesAsync();

        _currentUserAccessor.Setup(x => x.GetCurrentUsername()).Returns("observeruser");

        var command = new Add.Command("non-existent-user");

        // Act & Assert
        var exception = await Assert.ThrowsAsync<RestException>(
            async () => await _handler.Handle(command, CancellationToken.None)
        );

        exception.Code.Should().Be(System.Net.HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Handle_ShouldThrowRestException_WhenObserverNotFound()
    {
        // Arrange
        var target = new Person
        {
            Username = "targetuser",
            Email = "target@example.com"
        };
        Context.Persons.Add(target);
        await Context.SaveChangesAsync();

        _currentUserAccessor.Setup(x => x.GetCurrentUsername()).Returns("non-existent-user");

        var command = new Add.Command("targetuser");

        // Act & Assert
        var exception = await Assert.ThrowsAsync<RestException>(
            async () => await _handler.Handle(command, CancellationToken.None)
        );

        exception.Code.Should().Be(System.Net.HttpStatusCode.NotFound);
    }
}
