using EkoaGym.Application.Auth;
using EkoaGym.Application.Common.Configuration;
using EkoaGym.Application.Common.Exceptions;
using EkoaGym.Domain.Entities;
using EkoaGym.Infrastructure.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace EkoaGym.Infrastructure.Tests.Security;

public class AuthServiceTests
{
    private static AuthService CreateSut(out Persistence.EkoaGymDbContext dbContext)
    {
        dbContext = TestDbContextFactory.Create();
        var jwtOptions = Options.Create(new JwtOptions
        {
            SigningKey = "unit-test-signing-key-at-least-32-chars-long",
            Issuer = "EkoaGym.Tests",
            Audience = "EkoaGym.Tests",
            AccessTokenExpiryMinutes = 60,
            RefreshTokenExpiryDays = 30
        });

        return new AuthService(dbContext, new PasswordHasher<User>(), new JwtTokenGenerator(jwtOptions), jwtOptions);
    }

    [Fact]
    public async Task RegisterAsync_ValidRequest_CreatesUserAndReturnsTokens()
    {
        var sut = CreateSut(out var dbContext);
        var request = new RegisterRequest("new.user@example.com", "Password1", "New User");

        var response = await sut.RegisterAsync(request);

        Assert.False(string.IsNullOrWhiteSpace(response.AccessToken));
        Assert.False(string.IsNullOrWhiteSpace(response.RefreshToken));
        Assert.Single(dbContext.Users);
        Assert.Equal("new.user@example.com", dbContext.Users.Single().Email);
    }

    [Fact]
    public async Task RegisterAsync_DuplicateEmail_ThrowsEmailAlreadyInUseException()
    {
        var sut = CreateSut(out _);
        var request = new RegisterRequest("dup@example.com", "Password1", "First");
        await sut.RegisterAsync(request);

        await Assert.ThrowsAsync<EmailAlreadyInUseException>(
            () => sut.RegisterAsync(request with { Name = "Second" }));
    }

    [Fact]
    public async Task LoginAsync_WrongPassword_ThrowsInvalidCredentialsException()
    {
        var sut = CreateSut(out _);
        await sut.RegisterAsync(new RegisterRequest("login@example.com", "Password1", "User"));

        await Assert.ThrowsAsync<InvalidCredentialsException>(
            () => sut.LoginAsync(new LoginRequest("login@example.com", "WrongPassword1")));
    }

    [Fact]
    public async Task LoginAsync_UnknownEmail_ThrowsInvalidCredentialsException()
    {
        var sut = CreateSut(out _);

        await Assert.ThrowsAsync<InvalidCredentialsException>(
            () => sut.LoginAsync(new LoginRequest("unknown@example.com", "Password1")));
    }

    [Fact]
    public async Task LoginAsync_CorrectCredentials_ReturnsTokens()
    {
        var sut = CreateSut(out _);
        await sut.RegisterAsync(new RegisterRequest("ok@example.com", "Password1", "User"));

        var response = await sut.LoginAsync(new LoginRequest("ok@example.com", "Password1"));

        Assert.False(string.IsNullOrWhiteSpace(response.AccessToken));
    }

    [Fact]
    public async Task RefreshAsync_ValidToken_RotatesAndReturnsNewTokens()
    {
        var sut = CreateSut(out _);
        var registered = await sut.RegisterAsync(new RegisterRequest("refresh@example.com", "Password1", "User"));

        var refreshed = await sut.RefreshAsync(new RefreshRequest(registered.RefreshToken));

        Assert.NotEqual(registered.AccessToken, refreshed.AccessToken);
        Assert.NotEqual(registered.RefreshToken, refreshed.RefreshToken);
    }

    [Fact]
    public async Task RefreshAsync_ReusedToken_ThrowsInvalidRefreshTokenException()
    {
        var sut = CreateSut(out _);
        var registered = await sut.RegisterAsync(new RegisterRequest("reuse@example.com", "Password1", "User"));
        await sut.RefreshAsync(new RefreshRequest(registered.RefreshToken));

        // The original token was single-use; reusing it must fail even though it hasn't expired.
        await Assert.ThrowsAsync<InvalidRefreshTokenException>(
            () => sut.RefreshAsync(new RefreshRequest(registered.RefreshToken)));
    }

    [Fact]
    public async Task RefreshAsync_UnknownToken_ThrowsInvalidRefreshTokenException()
    {
        var sut = CreateSut(out _);

        await Assert.ThrowsAsync<InvalidRefreshTokenException>(
            () => sut.RefreshAsync(new RefreshRequest("not-a-real-token")));
    }
}
