using System.ComponentModel.DataAnnotations;

namespace EkoaGym.Application.Common.Validation;

// Ensures a DateOnly value is not in the future (e.g. a date of birth).
public class NotInFutureAttribute : ValidationAttribute
{
    public NotInFutureAttribute()
        : base("Date must not be in the future.")
    {
    }

    public override bool IsValid(object? value)
    {
        if (value is not DateOnly date)
        {
            return false;
        }

        return date <= DateOnly.FromDateTime(DateTime.UtcNow);
    }
}
