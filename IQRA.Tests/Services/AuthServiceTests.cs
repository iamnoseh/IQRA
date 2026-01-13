using System;
using System.Threading.Tasks;
using Application.Constants;
using Application.DTOs.Auth;
using Application.Interfaces;
using Domain.Entities.Users;
using Domain.Enums;
using Infrastructure.Data;
using Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace IQRA.Tests.Services;

public class AuthServiceTests
{
    private readonly Mock<UserManager<AppUser>> _userManagerMock;
    private readonly Mock<ApplicationDbContext> _contextMock;
    private readonly Mock<IJwtService> _jwtServiceMock;
    private readonly Mock<ISmsService> _smsServiceMock;
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
    private readonly Mock<ILogger<AuthService>> _loggerMock;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        var store = new Mock<IUserStore<AppUser>>();
        _userManagerMock = new Mock<UserManager<AppUser>>(store.Object, null, null, null, null, null, null, null, null);

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "IQRA_Test_Db")
            .Options;
        _contextMock = new Mock<ApplicationDbContext>(options);

        _jwtServiceMock = new Mock<IJwtService>();
        _smsServiceMock = new Mock<ISmsService>();
        _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        _loggerMock = new Mock<ILogger<AuthService>>();

        // Note: Mocking DbContext directly is hard, typically we use InMemory,
        // but since AuthService takes Context, we pass an instance or a mock.
        // For simple unit tests, we might need a real InMemory context if the service uses it directly.
        // Here I'm passing a context with in-memory options.
        var context = new ApplicationDbContext(options);

        _authService = new AuthService(
            _userManagerMock.Object,
            context,
            _jwtServiceMock.Object,
            _smsServiceMock.Object,
            _httpContextAccessorMock.Object,
            _loggerMock.Object
        );
    }

    [Fact]
    public async Task LoginAsync_InvalidCredentials_ReturnsError()
    {
        // Arrange
        var loginDto = new LoginDto { Username = "992900000000", Password = "wrongpassword" };
        _userManagerMock.Setup(x => x.FindByNameAsync(loginDto.Username)).ReturnsAsync((AppUser)null);

        // Act
        var result = await _authService.LoginAsync(loginDto);

        // Assert
        Assert.Equal(Guid.Empty, result.UserId);
        Assert.True(string.IsNullOrEmpty(result.Token));
    }

    [Fact]
    public async Task LoginAsync_ValidCredentials_ReturnsToken()
    {
        // Arrange
        var user = new AppUser { Id = Guid.NewGuid(), UserName = "992900000000", PhoneNumber = "992900000000", Role = UserRole.Student };
        var loginDto = new LoginDto { Username = "992900000000", Password = "password" };

        _userManagerMock.Setup(x => x.FindByNameAsync(loginDto.Username)).ReturnsAsync(user);
        _userManagerMock.Setup(x => x.CheckPasswordAsync(user, loginDto.Password)).ReturnsAsync(true);
        _jwtServiceMock.Setup(x => x.GenerateToken(user)).Returns("valid_token");

        // Act
        var result = await _authService.LoginAsync(loginDto);

        // Assert
        Assert.Equal("valid_token", result.Token);
        Assert.Equal(user.Id, result.UserId);
    }
}
