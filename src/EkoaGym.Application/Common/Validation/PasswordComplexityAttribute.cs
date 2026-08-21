using System.ComponentModel.DataAnnotations;

namespace EkoaGym.Application.Common.Validation;

// Requires 8+ chars with at least one letter and one digit.
public class PasswordComplexityAttribute : ValidationAttribute
{
    public PasswordComplexityAttribute()
        : base("Password must be at least 8 characters and contain at least one letter and one digit.")
    {
    }

    public override bool IsValid(object? value)
    {
        if (value is not string password)
        {
            return false;
        }

        return password.Length >= 8
            && password.Any(char.IsLetter)
            && password.Any(char.IsDigit);
    }
}
