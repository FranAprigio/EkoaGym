using System.ComponentModel.DataAnnotations;
using EkoaGym.Application.Common.Validation;

namespace EkoaGym.Application.BodyMetrics;

public record CreateBodyMetricRequest(
    [property: Required, NotInFuture] DateOnly Date,
    [property: Range(20, 400)] decimal WeightKg,
    [property: Range(10, 300)] decimal? WaistCm,
    [property: Range(10, 300)] decimal? ChestCm,
    [property: Range(10, 300)] decimal? ArmsCm);
