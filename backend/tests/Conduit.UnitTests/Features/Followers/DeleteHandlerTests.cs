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

public class DeleteHandlerTests : HandlerTestBase
{
    private readonly Mock<ICurrentUserAccessor> _currentUserAccessor;
    private readonly Mock<IProfileReader> _profileReader;
    private readonly Delete.QueryHandler _handler;

    public DeleteHandlerTests()
    {
        _currentUserAccessor = new Mock<ICurrentUserAccessor>();
        _profileReader = new Mock<IProfileReader>();
        _handler = new Delete.QueryHandler(Context, _currentUserAccessor.Object, _profileReader.Object);
    }

    [Fact]
    public async Task Handle_ShouldDeleteFollower_WhenValidDataProvided()
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
                IsFollowed = false
            }));

        var command = new Delete.Command("targetuser");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();

        // Verify the follow relationship was deleted
        var savedFollows = await Context.FollowedPeople.ToListAsync();
        savedFollows.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldBeIdempotent_WhenNotFollowing()
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
                IsFollowed = false
            }));

        var command = new Delete.Command("targetuser");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();

        // Verify no follow relationships exist
        var savedFollows = await Context.FollowedPeople.ToListAsync();
        savedFollows.Should().BeEmpty();
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

        var command = new Delete.Command("non-existent-user");

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

        var command = new Delete.Command("targetuser");

        // Act & Assert
        var exception = await Assert.ThrowsAsync<RestException>(
            async () => await _handler.Handle(command, CancellationToken.None)
        );

        exception.Code.Should().Be(System.Net.HttpStatusCode.NotFound);
    }
}
