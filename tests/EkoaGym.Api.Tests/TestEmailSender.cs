using System.Collections.Concurrent;
using EkoaGym.Application.Auth;

namespace EkoaGym.Api.Tests;

// Test double for IEmailSender so tests can retrieve the reset token that would otherwise only be logged.
public class TestEmailSender : IEmailSender
{
    private readonly ConcurrentDictionary<string, string> _lastResetTokenByEmail = new();

    public Task SendPasswordResetEmailAsync(string toEmail, string resetToken, CancellationToken cancellationToken = default)
    {
        _lastResetTokenByEmail[toEmail] = resetToken;
        return Task.CompletedTask;
    }

    public string GetLastResetToken(string email) => _lastResetTokenByEmail[email];
}
