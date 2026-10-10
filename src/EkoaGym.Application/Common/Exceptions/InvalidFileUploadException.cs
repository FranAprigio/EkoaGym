namespace EkoaGym.Application.Common.Exceptions;

// ErrorCode is stable (file_empty, file_too_large, image_type_not_allowed, proof_type_not_allowed); Message is the English fallback.
public class InvalidFileUploadException(string errorCode, string message, IReadOnlyDictionary<string, object?>? args = null)
    : Exception(message)
{
    public string ErrorCode { get; } = errorCode;

    public IReadOnlyDictionary<string, object?>? Args { get; } = args;
}
