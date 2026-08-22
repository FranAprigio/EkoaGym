using System.Globalization;

namespace EkoaGym.Application.Common.Exceptions;

public class WeightLimitExceededException(decimal maxWeightKg)
    : Exception($"This exercise has a maximum allowed weight of {maxWeightKg.ToString(CultureInfo.InvariantCulture)} kg.")
{
    public decimal MaxWeightKg { get; } = maxWeightKg;
}
