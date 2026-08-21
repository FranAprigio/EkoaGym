using System.ComponentModel.DataAnnotations;

namespace EkoaGym.Application.Common.Validation;

// Ensures a DateOnly value is today or later (e.g. a goal's target date).
public class NotInPastAttribute : ValidationAttribute
{
    public NotInPastAttribute()
        : base("Date must not be in the past.")
    {
    }

    public override bool IsValid(object? value)
    {
        if (value is not DateOnly date)
        {
            return false;
        }

        return date >= DateOnly.FromDateTime(DateTime.UtcNow);
    }
}
