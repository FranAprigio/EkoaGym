using EkoaGym.Application.BodyMetrics;
using EkoaGym.Domain.Entities;
using EkoaGym.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EkoaGym.Infrastructure.BodyMetrics;

public class BodyMetricService(EkoaGymDbContext dbContext) : IBodyMetricService
{
    public async Task<IReadOnlyList<BodyMetricResponse>> GetBodyMetricsAsync(Guid userId, BodyMetricFilter filter, CancellationToken cancellationToken = default)
    {
        var query = dbContext.BodyMetricLogs.Where(b => b.UserId == userId);

        if (filter.From is not null)
        {
            query = query.Where(b => b.Date >= filter.From.Value);
        }

        if (filter.To is not null)
        {
            query = query.Where(b => b.Date <= filter.To.Value);
        }

        return await query
            .OrderBy(b => b.Date)
            .Select(b => ToResponse(b))
            .ToListAsync(cancellationToken);
    }

    public async Task<BodyMetricResponse> CreateBodyMetricAsync(Guid userId, CreateBodyMetricRequest request, CancellationToken cancellationToken = default)
    {
        var log = new BodyMetricLog
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Date = request.Date,
            WeightKg = request.WeightKg,
            WaistCm = request.WaistCm,
            ChestCm = request.ChestCm,
            ArmsCm = request.ArmsCm,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        dbContext.BodyMetricLogs.Add(log);
        await dbContext.SaveChangesAsync(cancellationToken);

        return ToResponse(log);
    }

    private static BodyMetricResponse ToResponse(BodyMetricLog log) => new(
        log.Id, log.Date, log.WeightKg, log.WaistCm, log.ChestCm, log.ArmsCm, log.CreatedAt, log.UpdatedAt);
}
