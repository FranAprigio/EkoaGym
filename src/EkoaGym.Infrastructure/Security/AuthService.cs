using System.Security.Cryptography;
using EkoaGym.Application.Auth;
using EkoaGym.Application.Common.Configuration;
using EkoaGym.Application.Common.Exceptions;
using EkoaGym.Domain.Entities;
using EkoaGym.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace EkoaGym.Infrastructure.Security;

public class AuthService(
    EkoaGymDbContext dbContext,
    IPasswordHasher<User> passwordHasher,
    IJwtTokenGenerator jwtTokenGenerator,
    IEmailSender emailSender,
    IOptions<JwtOptions> jwtOptions) : IAuthService
{
    private static readonly TimeSpan ResetTokenLifetime = TimeSpan.FromHours(1);

    private readonly JwtOptions _jwtOptions = jwtOptions.Value;

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        var emailExists = await dbContext.Users.AnyAsync(u => u.Email == normalizedEmail, cancellationToken);
        if (emailExists)
        {
            throw new EmailAlreadyInUseException(normalizedEmail);
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = normalizedEmail,
            Name = request.Name.Trim(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        user.PasswordHash = passwordHasher.HashPassword(user, request.Password);

        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync(cancellationToken);

        return await IssueTokensAsync(user, cancellationToken);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        var user = await dbContext.Users.SingleOrDefaultAsync(u => u.Email == normalizedEmail && !u.IsDeleted, cancellationToken);
        if (user is null)
        {
            throw new InvalidCredentialsException();
        }

        var verificationResult = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (verificationResult == PasswordVerificationResult.Failed)
        {
            throw new InvalidCredentialsException();
        }

        return await IssueTokensAsync(user, cancellationToken);
    }

    public async Task<AuthResponse> RefreshAsync(RefreshRequest request, CancellationToken cancellationToken = default)
    {
        var tokenHash = HashToken(request.RefreshToken);

        var existingToken = await dbContext.RefreshTokens
            .Include(rt => rt.User)
            .SingleOrDefaultAsync(rt => rt.TokenHash == tokenHash, cancellationToken);

        if (existingToken is null || existingToken.IsRevoked || existingToken.ExpiresAt < DateTime.UtcNow)
        {
            throw new InvalidRefreshTokenException();
        }

        // Rotate: the presented token is single-use, revoke it before issuing a replacement.
        existingToken.IsRevoked = true;

        return await IssueTokensAsync(existingToken.User, cancellationToken);
    }

    public async Task ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        var user = await dbContext.Users.SingleOrDefaultAsync(u => u.Email == normalizedEmail && !u.IsDeleted, cancellationToken);
        if (user is null)
        {
            // Never reveal whether the email is registered — caller always sees the same generic outcome.
            return;
        }

        var resetToken = GenerateRefreshToken();
        dbContext.PasswordResetTokens.Add(new PasswordResetToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = HashToken(resetToken),
            ExpiresAt = DateTime.UtcNow.Add(ResetTokenLifetime),
            CreatedAt = DateTime.UtcNow
        });
        await dbContext.SaveChangesAsync(cancellationToken);

        await emailSender.SendPasswordResetEmailAsync(user.Email, resetToken, cancellationToken);
    }

    public async Task ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken = default)
    {
        var tokenHash = HashToken(request.Token);

        var resetToken = await dbContext.PasswordResetTokens
            .Include(t => t.User)
            .SingleOrDefaultAsync(t => t.TokenHash == tokenHash, cancellationToken);

        if (resetToken is null || resetToken.IsUsed || resetToken.ExpiresAt < DateTime.UtcNow)
        {
            throw new InvalidPasswordResetTokenException();
        }

        resetToken.IsUsed = true;
        resetToken.User.PasswordHash = passwordHasher.HashPassword(resetToken.User, request.NewPassword);
        resetToken.User.UpdatedAt = DateTime.UtcNow;

        // Force re-login everywhere: revoke all outstanding refresh tokens for this user.
        var activeRefreshTokens = await dbContext.RefreshTokens
            .Where(rt => rt.UserId == resetToken.UserId && !rt.IsRevoked)
            .ToListAsync(cancellationToken);
        foreach (var refreshToken in activeRefreshTokens)
        {
            refreshToken.IsRevoked = true;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<AuthResponse> IssueTokensAsync(User user, CancellationToken cancellationToken)
    {
        var (accessToken, accessTokenExpiresAt) = jwtTokenGenerator.GenerateAccessToken(user);

        var refreshToken = GenerateRefreshToken();
        dbContext.RefreshTokens.Add(new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = HashToken(refreshToken),
            ExpiresAt = DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenExpiryDays),
            CreatedAt = DateTime.UtcNow
        });

        await dbContext.SaveChangesAsync(cancellationToken);

        return new AuthResponse(accessToken, refreshToken, accessTokenExpiresAt);
    }

    private static string GenerateRefreshToken() => Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

    private static string HashToken(string token) => Convert.ToHexString(SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(token)));
}
