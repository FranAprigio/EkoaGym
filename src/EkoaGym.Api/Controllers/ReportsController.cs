using EkoaGym.Api.Common;
using EkoaGym.Application.Reports;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EkoaGym.Api.Controllers;

[ApiController]
[Route("api/reports")]
[Authorize]
public class ReportsController(IReportService reportService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Submit(CreateReportRequest request, CancellationToken cancellationToken)
    {
        await reportService.SubmitReportAsync(User.GetUserId(), request, cancellationToken);
        // Always the same generic ack — never reveals whether it led to manual review or a shadowban.
        return Ok(new { message = "Thanks, we'll take a look." });
    }
}
