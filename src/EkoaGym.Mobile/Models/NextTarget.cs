namespace EkoaGym.Mobile.Models;

// Client mirror of GET api/plans/{planId}/exercises/{exerciseId}/next-target.
public record NextTarget(int Sets, int Reps, decimal WeightKg);
