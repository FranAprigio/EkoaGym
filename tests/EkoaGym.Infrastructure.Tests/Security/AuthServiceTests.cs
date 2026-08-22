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
    private sealed class FakeEmailSender : IEmailSender
    {
        public string? LastRecipient { get; private set; }
        public string? LastResetToken { get; private set; }
        public int SendCount { get; private set; }

        public Task SendPasswordResetEmailAsync(string toEmail, string resetToken, CancellationToken cancellationToken = default)
        {
            SendCount++;
            LastRecipient = toEmail;
            LastResetToken = resetToken;
            return Task.CompletedTask;
        }
    }

    private static AuthService CreateSut(out Persistence.EkoaGymDbContext dbContext, out FakeEmailSender emailSender)
    {
        dbContext = TestDbContextFactory.Create();
        emailSender = new FakeEmailSender();
        var jwtOptions = Options.Create(new JwtOptions
        {
            SigningKey = "unit-test-signing-key-at-least-32-chars-long",
            Issuer = "EkoaGym.Tests",
            Audience = "EkoaGym.Tests",
            AccessTokenExpiryMinutes = 60,
            RefreshTokenExpiryDays = 30
        });

        return new AuthService(dbContext, new PasswordHasher<User>(), new JwtTokenGenerator(jwtOptions), emailSender, jwtOptions);
    }

    private static AuthService CreateSut(out Persistence.EkoaGymDbContext dbContext) => CreateSut(out dbContext, out _);

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

    [Fact]
    public async Task ForgotPasswordAsync_KnownEmail_CreatesTokenAndSendsEmail()
    {
        var sut = CreateSut(out var dbContext, out var emailSender);
        await sut.RegisterAsync(new RegisterRequest("forgot@example.com", "Password1", "User"));

        await sut.ForgotPasswordAsync(new ForgotPasswordRequest("forgot@example.com"));

        Assert.Equal(1, emailSender.SendCount);
        Assert.Equal("forgot@example.com", emailSender.LastRecipient);
        Assert.False(string.IsNullOrWhiteSpace(emailSender.LastResetToken));
        Assert.Single(dbContext.PasswordResetTokens);
    }

    [Fact]
    public async Task ForgotPasswordAsync_UnknownEmail_DoesNotSendEmailOrCreateToken()
    {
        var sut = CreateSut(out var dbContext, out var emailSender);

        await sut.ForgotPasswordAsync(new ForgotPasswordRequest("nobody@example.com"));

        Assert.Equal(0, emailSender.SendCount);
        Assert.Empty(dbContext.PasswordResetTokens);
    }

    [Fact]
    public async Task ResetPasswordAsync_ValidToken_UpdatesPasswordAndRevokesRefreshTokens()
    {
        var sut = CreateSut(out var dbContext, out var emailSender);
        var registered = await sut.RegisterAsync(new RegisterRequest("reset@example.com", "Password1", "User"));
        await sut.ForgotPasswordAsync(new ForgotPasswordRequest("reset@example.com"));

        await sut.ResetPasswordAsync(new ResetPasswordRequest(emailSender.LastResetToken!, "NewPassword2"));

        // Old password no longer works, new one does.
        await Assert.ThrowsAsync<InvalidCredentialsException>(
            () => sut.LoginAsync(new LoginRequest("reset@example.com", "Password1")));
        await sut.LoginAsync(new LoginRequest("reset@example.com", "NewPassword2"));

        // The refresh token issued at registration must have been revoked by the reset.
        await Assert.ThrowsAsync<InvalidRefreshTokenException>(
            () => sut.RefreshAsync(new RefreshRequest(registered.RefreshToken)));
    }

    [Fact]
    public async Task ResetPasswordAsync_ReusedToken_ThrowsInvalidPasswordResetTokenException()
    {
        var sut = CreateSut(out _, out var emailSender);
        await sut.RegisterAsync(new RegisterRequest("reused-reset@example.com", "Password1", "User"));
        await sut.ForgotPasswordAsync(new ForgotPasswordRequest("reused-reset@example.com"));
        await sut.ResetPasswordAsync(new ResetPasswordRequest(emailSender.LastResetToken!, "NewPassword2"));

        await Assert.ThrowsAsync<InvalidPasswordResetTokenException>(
            () => sut.ResetPasswordAsync(new ResetPasswordRequest(emailSender.LastResetToken!, "AnotherPassword3")));
    }

    [Fact]
    public async Task ResetPasswordAsync_UnknownToken_ThrowsInvalidPasswordResetTokenException()
    {
        var sut = CreateSut(out _);

        await Assert.ThrowsAsync<InvalidPasswordResetTokenException>(
            () => sut.ResetPasswordAsync(new ResetPasswordRequest("not-a-real-token", "NewPassword2")));
    }
}
