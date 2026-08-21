namespace EkoaGym.Mobile.Models;

// Known suggestions surfaced in the UI; "Custom" reveals a free-text field whose value becomes the actual Type sent to the API.
public static class GoalTypes
{
    public const string WeightLoss = "WeightLoss";
    public const string WeightGain = "WeightGain";
    public const string MuscleGain = "MuscleGain";
    public const string Endurance = "Endurance";
    public const string Custom = "Custom";

    public static readonly IReadOnlyList<string> KnownTypes = [WeightLoss, WeightGain, MuscleGain, Endurance, Custom];
}
