namespace EkoaGym.Application.Reports;

public interface IReportService
{
    // Always resolves to the same generic acknowledgment — never reveals whether it led to a manual
    // review or an automatic shadowban (M-10.5).
    Task SubmitReportAsync(Guid reporterId, CreateReportRequest request, CancellationToken cancellationToken = default);
}
