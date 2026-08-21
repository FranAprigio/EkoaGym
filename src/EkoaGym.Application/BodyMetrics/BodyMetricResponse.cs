namespace EkoaGym.Application.BodyMetrics;

public record BodyMetricResponse(
    Guid Id,
    DateOnly Date,
    decimal WeightKg,
    decimal? WaistCm,
    decimal? ChestCm,
    decimal? ArmsCm,
    DateTime CreatedAt,
    DateTime UpdatedAt);
