using EkoaGym.Application.Common.Exceptions;
using EkoaGym.Application.Reports;
using EkoaGym.Domain.Entities;
using EkoaGym.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EkoaGym.Infrastructure.Reports;

public class ReportService(EkoaGymDbContext dbContext) : IReportService
{
    private const int MaxReportsPerWindow = 5;
    private static readonly TimeSpan RateLimitWindow = TimeSpan.FromHours(24);

    public async Task SubmitReportAsync(Guid reporterId, CreateReportRequest request, CancellationToken cancellationToken = default)
    {
        var targetExists = await dbContext.Users.AnyAsync(u => u.Id == request.TargetUserId && !u.IsDeleted, cancellationToken);
        if (!targetExists)
        {
            throw new TargetUserNotFoundException();
        }

        var windowStart = DateTime.UtcNow.Subtract(RateLimitWindow);
        var recentReportCount = await dbContext.Reports
            .CountAsync(r => r.ReporterId == reporterId && r.CreatedAt >= windowStart, cancellationToken);
        if (recentReportCount >= MaxReportsPerWindow)
        {
            throw new ReportRateLimitExceededException();
        }

        dbContext.Reports.Add(new Report
        {
            Id = Guid.NewGuid(),
            ReporterId = reporterId,
            TargetUserId = request.TargetUserId,
            Reason = request.Reason,
            CreatedAt = DateTime.UtcNow
        });

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
