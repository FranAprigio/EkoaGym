namespace EkoaGym.Api.ExceptionHandling;

// Shape of all error responses returned by the API. Code is stable (clients map it to their own text);
// Message is an English fallback; Args carries values the localized text needs (e.g. maxWeightKg).
public record ApiErrorResponse(string Code, string Message, IReadOnlyDictionary<string, object?>? Args = null);

public record ValidationFieldError(string Field, string Code, string Message);

// 400 body for model-validation failures. "Errors" keeps the legacy {field: [messages]} shape older app versions parse.
public record ValidationErrorResponse(
    string Code,
    string Message,
    IReadOnlyList<ValidationFieldError> FieldErrors,
    IReadOnlyDictionary<string, string[]> Errors);
