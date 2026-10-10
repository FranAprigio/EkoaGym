using System.ComponentModel.DataAnnotations;
using EkoaGym.Domain.Constants;

namespace EkoaGym.Application.Common.Validation;

// Null is allowed (field is optional); anything else must be a supported language code.
public class SupportedLanguageAttribute : ValidationAttribute
{
    public SupportedLanguageAttribute()
        : base("unsupported_language")
    {
    }

    public override bool IsValid(object? value) =>
        value is null || (value is string code && SupportedLanguages.Normalize(code) is not null);
}
