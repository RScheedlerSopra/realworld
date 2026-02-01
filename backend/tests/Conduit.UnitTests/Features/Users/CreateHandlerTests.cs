using System;
using System.Linq;
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

public class CreateHandlerTests : HandlerTestBase
{
    private readonly Mock<IPasswordHasher> _passwordHasher;
    private readonly Mock<IJwtTokenGenerator> _jwtTokenGenerator;
    private readonly Mock<IMapper> _mapper;
    private readonly Create.Handler _handler;

    public CreateHandlerTests()
    {
        _passwordHasher = new Mock<IPasswordHasher>();
        _jwtTokenGenerator = new Mock<IJwtTokenGenerator>();
        _mapper = new Mock<IMapper>();
        _handler = new Create.Handler(Context, _passwordHasher.Object, _jwtTokenGenerator.Object, _mapper.Object);
    }

    [Fact]
    public async Task Handle_ShouldCreateUser_WhenValidDataProvided()
    {
        // Arrange
        var hashedPassword = new byte[] { 1, 2, 3, 4 };
        _passwordHasher.Setup(x => x.Hash(It.IsAny<string>(), It.IsAny<byte[]>()))
            .ReturnsAsync(hashedPassword);

        _jwtTokenGenerator.Setup(x => x.CreateToken("testuser"))
            .Returns("test-token");

        _mapper.Setup(x => x.Map<Person, User>(It.IsAny<Person>()))
            .Returns((Person p) => new User
            {
                Username = p.Username,
                Email = p.Email,
                Bio = p.Bio,
                Image = p.Image,
                Token = null
            });

        var command = new Create.Command(
            User: new Create.UserData(Username: "testuser", Email: "test@example.com", Password: "password")
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.User.Should().NotBeNull();
        result.User!.Username.Should().Be("testuser");
        result.User.Email.Should().Be("test@example.com");
        result.User.Token.Should().Be("test-token");

        // Verify the user was saved to the database
        var savedUser = await Context.Persons.FirstOrDefaultAsync(x => x.Username == "testuser");
        savedUser.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_ShouldThrowRestException_WhenUsernameAlreadyExists()
    {
        // Arrange
        var existingUser = new Person
        {
            Username = "testuser",
            Email = "existing@example.com",
            Hash = [1, 2, 3],
            Salt = [4, 5, 6]
        };
        Context.Persons.Add(existingUser);
        await Context.SaveChangesAsync();

        var command = new Create.Command(
            User: new Create.UserData(Username: "testuser", Email: "newemail@example.com", Password: "password")
        );

        // Act & Assert
        var exception = await Assert.ThrowsAsync<RestException>(
            async () => await _handler.Handle(command, CancellationToken.None)
        );

        exception.Code.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Handle_ShouldThrowRestException_WhenEmailAlreadyExists()
    {
        // Arrange
        var existingUser = new Person
        {
            Username = "existinguser",
            Email = "test@example.com",
            Hash = [1, 2, 3],
            Salt = [4, 5, 6]
        };
        Context.Persons.Add(existingUser);
        await Context.SaveChangesAsync();

        var command = new Create.Command(
            User: new Create.UserData(Username: "newuser", Email: "test@example.com", Password: "password")
        );

        // Act & Assert
        var exception = await Assert.ThrowsAsync<RestException>(
            async () => await _handler.Handle(command, CancellationToken.None)
        );

        exception.Code.Should().Be(System.Net.HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Handle_ShouldHashPassword()
    {
        // Arrange
        var hashedPassword = new byte[] { 1, 2, 3, 4 };
        _passwordHasher.Setup(x => x.Hash("password", It.IsAny<byte[]>()))
            .ReturnsAsync(hashedPassword);

        _jwtTokenGenerator.Setup(x => x.CreateToken(It.IsAny<string>()))
            .Returns("test-token");

        _mapper.Setup(x => x.Map<Person, User>(It.IsAny<Person>()))
            .Returns(new User { Username = "testuser", Email = "test@example.com" });

        var command = new Create.Command(
            User: new Create.UserData(Username: "testuser", Email: "test@example.com", Password: "password")
        );

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _passwordHasher.Verify(x => x.Hash("password", It.IsAny<byte[]>()), Times.Once);

        var savedUser = await Context.Persons.FirstOrDefaultAsync(x => x.Username == "testuser");
        savedUser!.Hash.Should().Equal(hashedPassword);
    }
}
