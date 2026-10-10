using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;

namespace EkoaGym.Api.ExceptionHandling;

// Built-in DataAnnotations attributes carry English sentences; swap them for stable codes so clients can localize.
// Custom attributes already pass their code through the base constructor, so they are left alone.
public class StableValidationCodesProvider : IValidationMetadataProvider
{
    public void CreateValidationMetadata(ValidationMetadataProviderContext context)
    {
        foreach (var attribute in context.ValidationMetadata.ValidatorMetadata.OfType<ValidationAttribute>())
        {
            if (attribute.ErrorMessage is not null || attribute.ErrorMessageResourceName is not null)
            {
                continue;
            }

            attribute.ErrorMessage = attribute switch
            {
                RequiredAttribute => "required",
                EmailAddressAttribute => "invalid_email",
                StringLengthAttribute => "invalid_length",
                MinLengthAttribute or MaxLengthAttribute => "invalid_length",
                RangeAttribute => "out_of_range",
                CompareAttribute => "values_do_not_match",
                _ => "invalid"
            };
        }
    }
}

public static class ValidationErrorFactory
{
    private static readonly IReadOnlyDictionary<string, string> EnglishFallbacks = new Dictionary<string, string>
    {
        ["required"] = "This field is required.",
        ["invalid_email"] = "Enter a valid email address.",
        ["invalid_length"] = "This value has an invalid length.",
        ["out_of_range"] = "This value is out of range.",
        ["values_do_not_match"] = "The values do not match.",
        ["password_too_weak"] = "Password must be at least 8 characters and contain at least one letter and one digit.",
        ["date_in_future"] = "Date must not be in the future.",
        ["date_in_past"] = "Date must not be in the past.",
        ["unsupported_language"] = "Language is not supported.",
        ["invalid"] = "This value is invalid."
    };

    public static ValidationErrorResponse Create(Microsoft.AspNetCore.Mvc.ModelBinding.ModelStateDictionary modelState)
    {
        var fieldErrors = modelState
            .Where(entry => entry.Value?.Errors.Count > 0)
            .SelectMany(entry => entry.Value!.Errors.Select(error =>
            {
                var code = IsStableCode(error.ErrorMessage) ? error.ErrorMessage : "invalid";
                return new ValidationFieldError(ToCamelCase(entry.Key), code, EnglishFallbacks.GetValueOrDefault(code, EnglishFallbacks["invalid"]));
            }))
            .ToList();

        var errors = fieldErrors
            .GroupBy(e => e.Field)
            .ToDictionary(g => g.Key, g => g.Select(e => e.Message).ToArray());

        return new ValidationErrorResponse("validation_error", "One or more fields are invalid.", fieldErrors, errors);
    }

    private static bool IsStableCode(string message) =>
        message.Length > 0 && message.All(c => c is >= 'a' and <= 'z' or '_');

    private static string ToCamelCase(string key) =>
        key.Length == 0 ? key : string.Concat(char.ToLowerInvariant(key[0]).ToString(), key.AsSpan(1));
}
