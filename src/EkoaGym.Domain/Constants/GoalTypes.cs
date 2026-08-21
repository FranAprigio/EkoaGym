namespace EkoaGym.Domain.Constants;

// Known goal types surfaced to clients as suggestions; any other non-empty value is accepted as a custom goal.
public static class GoalTypes
{
    public const string WeightLoss = "WeightLoss";
    public const string WeightGain = "WeightGain";
    public const string MuscleGain = "MuscleGain";
    public const string Endurance = "Endurance";
    public const string Custom = "Custom";

    public static readonly IReadOnlyList<string> KnownTypes = [WeightLoss, WeightGain, MuscleGain, Endurance, Custom];
}
