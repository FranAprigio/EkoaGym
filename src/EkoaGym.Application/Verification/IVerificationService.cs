namespace EkoaGym.Application.Verification;

public interface IVerificationService
{
    Task<VerificationResponse> SubmitAsync(Guid userId, VerificationUpload upload, CancellationToken cancellationToken = default);
}
