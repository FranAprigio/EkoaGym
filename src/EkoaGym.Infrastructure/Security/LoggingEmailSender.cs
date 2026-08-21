using EkoaGym.Application.Auth;
using Microsoft.Extensions.Logging;

namespace EkoaGym.Infrastructure.Security;

// Dev-mode stand-in: no SMTP/email provider is wired up yet, so the reset link is just logged.
// Swap this for a real provider (e.g. SendGrid/SES) before shipping the forgot-password flow to production.
public class LoggingEmailSender(ILogger<LoggingEmailSender> logger) : IEmailSender
{
    public Task SendPasswordResetEmailAsync(string toEmail, string resetToken, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Password reset requested for {Email}. Reset token: {ResetToken}", toEmail, resetToken);
        return Task.CompletedTask;
    }
}
