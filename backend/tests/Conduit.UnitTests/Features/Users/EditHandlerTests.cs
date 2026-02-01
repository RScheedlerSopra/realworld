using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Conduit.Domain;
using Conduit.Features.Users;
using Conduit.Infrastructure;
using Conduit.Infrastructure.Errors;
using Conduit.Infrastructure.Security;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace Conduit.UnitTests.Features.Users;

public class EditHandlerTests : HandlerTestBase
{
    private readonly Mock<IPasswordHasher> _passwordHasher;
    private readonly Mock<ICurrentUserAccessor> _currentUserAccessor;
    private readonly Mock<IMapper> _mapper;
    private readonly Edit.Handler _handler;

    public EditHandlerTests()
    {
        _passwordHasher = new Mock<IPasswordHasher>();
        _currentUserAccessor = new Mock<ICurrentUserAccessor>();
        _mapper = new Mock<IMapper>();
        _handler = new Edit.Handler(Context, _passwordHasher.Object, _currentUserAccessor.Object, _mapper.Object);
    }

    [Fact]
    public async Task Handle_ShouldUpdateUser_WhenValidDataProvided()
    {
        // Arrange
        var user = new Person
        {
            Username = "testuser",
            Email = "test@example.com",
            Bio = "Old bio",
            Image = "http://example.com/old.jpg",
            Hash = [1, 2, 3],
            Salt = [4, 5, 6]
        };
        Context.Persons.Add(user);
        await Context.SaveChangesAsync();

        _currentUserAccessor.Setup(x => x.GetCurrentUsername()).Returns("testuser");

        _mapper.Setup(x => x.Map<Person, User>(It.IsAny<Person>()))
            .Returns((Person p) => new User
            {
                Username = p.Username,
                Email = p.Email,
                Bio = p.Bio,
                Image = p.Image,
                Token = null
            });

        var command = new Edit.Command(
            User: new Edit.UserData
            {
                Username = "newusername",
                Email = "newemail@example.com",
                Bio = "New bio",
                Image = "http://example.com/new.jpg",
                Password = null
            }
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.User.Should().NotBeNull();
        result.User!.Username.Should().Be("newusername");
        result.User.Email.Should().Be("newemail@example.com");
        result.User.Bio.Should().Be("New bio");
        result.User.Image.Should().Be("http://example.com/new.jpg");

        // Verify the user was updated in the database
        var updatedUser = await Context.Persons.FirstOrDefaultAsync(x => x.Username == "newusername");
        updatedUser.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_ShouldUpdatePassword_WhenPasswordProvided()
    {
        // Arrange
        var user = new Person
        {
            Username = "testuser",
            Email = "test@example.com",
            Hash = [1, 2, 3],
            Salt = [4, 5, 6]
        };
        Context.Persons.Add(user);
        await Context.SaveChangesAsync();

        var newHash = new byte[] { 7, 8, 9 };
        _passwordHasher.Setup(x => x.Hash("newpassword", It.IsAny<byte[]>()))
            .ReturnsAsync(newHash);

        _currentUserAccessor.Setup(x => x.GetCurrentUsername()).Returns("testuser");

        _mapper.Setup(x => x.Map<Person, User>(It.IsAny<Person>()))
            .Returns(new User { Username = "testuser", Email = "test@example.com" });

        var command = new Edit.Command(
            User: new Edit.UserData
            {
                Username = null,
                Email = null,
                Bio = null,
                Image = null,
                Password = "newpassword"
            }
        );

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _passwordHasher.Verify(x => x.Hash("newpassword", It.IsAny<byte[]>()), Times.Once);

        var updatedUser = await Context.Persons.FirstOrDefaultAsync(x => x.Username == "testuser");
        updatedUser!.Hash.Should().Equal(newHash);
    }

    [Fact]
    public async Task Handle_ShouldNotUpdatePassword_WhenPasswordIsNull()
    {
        // Arrange
        var originalHash = new byte[] { 1, 2, 3 };
        var originalSalt = new byte[] { 4, 5, 6 };
        var user = new Person
        {
            Username = "testuser",
            Email = "test@example.com",
            Hash = originalHash,
            Salt = originalSalt
        };
        Context.Persons.Add(user);
        await Context.SaveChangesAsync();

        _currentUserAccessor.Setup(x => x.GetCurrentUsername()).Returns("testuser");

        _mapper.Setup(x => x.Map<Person, User>(It.IsAny<Person>()))
            .Returns(new User { Username = "testuser", Email = "test@example.com" });

        var command = new Edit.Command(
            User: new Edit.UserData
            {
                Username = null,
                Email = null,
                Bio = "New bio",
                Image = null,
                Password = null
            }
        );

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _passwordHasher.Verify(x => x.Hash(It.IsAny<string>(), It.IsAny<byte[]>()), Times.Never);

        var updatedUser = await Context.Persons.FirstOrDefaultAsync(x => x.Username == "testuser");
        updatedUser!.Hash.Should().Equal(originalHash);
        updatedUser!.Salt.Should().Equal(originalSalt);
    }

    [Fact]
    public async Task Handle_ShouldThrowRestException_WhenUserNotFound()
    {
        // Arrange
        _currentUserAccessor.Setup(x => x.GetCurrentUsername()).Returns("nonexistent");

        var command = new Edit.Command(
            User: new Edit.UserData { Bio = "New bio" }
        );

        // Act & Assert
        var exception = await Assert.ThrowsAsync<RestException>(
            async () => await _handler.Handle(command, CancellationToken.None)
        );

        exception.Code.Should().Be(System.Net.HttpStatusCode.NotFound);
    }
}
