using System.ComponentModel.DataAnnotations;
using EkoaGym.Application.Common.Validation;

namespace EkoaGym.Application.BodyMetrics;

public record CreateBodyMetricRequest(
    [Required, NotInFuture] DateOnly Date,
    [Range(20, 400)] decimal WeightKg,
    [Range(10, 300)] decimal? WaistCm,
    [Range(10, 300)] decimal? ChestCm,
    [Range(10, 300)] decimal? ArmsCm);
