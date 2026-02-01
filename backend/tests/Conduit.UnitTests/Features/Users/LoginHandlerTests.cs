using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Conduit.Domain;
using Conduit.Features.Users;
using Conduit.Infrastructure.Errors;
using Conduit.Infrastructure.Security;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace Conduit.UnitTests.Features.Users;

public class LoginHandlerTests : HandlerTestBase
{
    private readonly Mock<IPasswordHasher> _passwordHasher;
    private readonly Mock<IJwtTokenGenerator> _jwtTokenGenerator;
    private readonly Mock<IMapper> _mapper;
    private readonly Login.Handler _handler;

    public LoginHandlerTests()
    {
        _passwordHasher = new Mock<IPasswordHasher>();
        _jwtTokenGenerator = new Mock<IJwtTokenGenerator>();
        _mapper = new Mock<IMapper>();
        _handler = new Login.Handler(Context, _passwordHasher.Object, _jwtTokenGenerator.Object, _mapper.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnUser_WhenCredentialsAreValid()
    {
        // Arrange
        var salt = new byte[] { 1, 2, 3 };
        var hash = new byte[] { 4, 5, 6 };
        var user = new Person
        {
            Username = "testuser",
            Email = "test@example.com",
            Hash = hash,
            Salt = salt
        };
        Context.Persons.Add(user);
        await Context.SaveChangesAsync();

        _passwordHasher.Setup(x => x.Hash("password", salt))
            .ReturnsAsync(hash);

        _jwtTokenGenerator.Setup(x => x.CreateToken("testuser"))
            .Returns("test-token");

        _mapper.Setup(x => x.Map<Person, User>(It.IsAny<Person>()))
            .Returns(new User
            {
                Username = "testuser",
                Email = "test@example.com",
                Bio = null,
                Image = null,
                Token = null
            });

        var command = new Login.Command(
            User: new Login.UserData { Email = "test@example.com", Password = "password" }
        );

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.User.Should().NotBeNull();
        result.User!.Username.Should().Be("testuser");
        result.User.Email.Should().Be("test@example.com");
        result.User.Token.Should().Be("test-token");
    }

    [Fact]
    public async Task Handle_ShouldThrowRestException_WhenEmailNotFound()
    {
        // Arrange
        var command = new Login.Command(
            User: new Login.UserData { Email = "nonexistent@example.com", Password = "password" }
        );

        // Act & Assert
        var exception = await Assert.ThrowsAsync<RestException>(
            async () => await _handler.Handle(command, CancellationToken.None)
        );

        exception.Code.Should().Be(System.Net.HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Handle_ShouldThrowRestException_WhenPasswordIsIncorrect()
    {
        // Arrange
        var salt = new byte[] { 1, 2, 3 };
        var hash = new byte[] { 4, 5, 6 };
        var user = new Person
        {
            Username = "testuser",
            Email = "test@example.com",
            Hash = hash,
            Salt = salt
        };
        Context.Persons.Add(user);
        await Context.SaveChangesAsync();

        var wrongHash = new byte[] { 7, 8, 9 };
        _passwordHasher.Setup(x => x.Hash("wrongpassword", salt))
            .ReturnsAsync(wrongHash);

        var command = new Login.Command(
            User: new Login.UserData { Email = "test@example.com", Password = "wrongpassword" }
        );

        // Act & Assert
        var exception = await Assert.ThrowsAsync<RestException>(
            async () => await _handler.Handle(command, CancellationToken.None)
        );

        exception.Code.Should().Be(System.Net.HttpStatusCode.Unauthorized);
    }
}
