namespace EkoaGym.Mobile.Models;

public record BodyMetricLog(Guid Id, DateOnly Date, decimal WeightKg, decimal? WaistCm, decimal? ChestCm, decimal? ArmsCm, DateTime CreatedAt, DateTime UpdatedAt);

public record CreateBodyMetricRequest(DateOnly Date, decimal WeightKg, decimal? WaistCm, decimal? ChestCm, decimal? ArmsCm);
