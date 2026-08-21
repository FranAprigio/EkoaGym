namespace EkoaGym.Application.Auth;

// Abstraction over however reset links actually get delivered; Infrastructure provides the real implementation.
public interface IEmailSender
{
    Task SendPasswordResetEmailAsync(string toEmail, string resetToken, CancellationToken cancellationToken = default);
}
