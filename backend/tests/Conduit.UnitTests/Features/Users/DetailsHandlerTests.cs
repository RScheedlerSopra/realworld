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

public class DetailsHandlerTests : HandlerTestBase
{
    private readonly Mock<IJwtTokenGenerator> _jwtTokenGenerator;
    private readonly Mock<IMapper> _mapper;
    private readonly Details.QueryHandler _handler;

    public DetailsHandlerTests()
    {
        _jwtTokenGenerator = new Mock<IJwtTokenGenerator>();
        _mapper = new Mock<IMapper>();
        _handler = new Details.QueryHandler(Context, _jwtTokenGenerator.Object, _mapper.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnUser_WhenUserExists()
    {
        // Arrange
        var person = new Person
        {
            Username = "testuser",
            Email = "test@example.com",
            Bio = "Test bio",
            Image = "http://example.com/image.jpg",
            Hash = [1, 2, 3],
            Salt = [4, 5, 6]
        };
        Context.Persons.Add(person);
        await Context.SaveChangesAsync();

        _jwtTokenGenerator.Setup(x => x.CreateToken("testuser"))
            .Returns("test-token");

        _mapper.Setup(x => x.Map<Person, User>(It.IsAny<Person>()))
            .Returns(new User
            {
                Username = "testuser",
                Email = "test@example.com",
                Bio = "Test bio",
                Image = "http://example.com/image.jpg",
                Token = null
            });

        var query = new Details.Query("testuser");

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.User.Should().NotBeNull();
        result.User!.Username.Should().Be("testuser");
        result.User.Email.Should().Be("test@example.com");
        result.User.Bio.Should().Be("Test bio");
        result.User.Image.Should().Be("http://example.com/image.jpg");
        result.User.Token.Should().Be("test-token");
    }

    [Fact]
    public async Task Handle_ShouldThrowRestException_WhenUserNotFound()
    {
        // Arrange
        var query = new Details.Query("nonexistent");

        // Act & Assert
        var exception = await Assert.ThrowsAsync<RestException>(
            async () => await _handler.Handle(query, CancellationToken.None)
        );

        exception.Code.Should().Be(System.Net.HttpStatusCode.NotFound);
    }
}
