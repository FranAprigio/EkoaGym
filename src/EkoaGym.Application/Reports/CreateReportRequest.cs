using System.ComponentModel.DataAnnotations;

namespace EkoaGym.Application.Reports;

public record CreateReportRequest(
    [Required] Guid TargetUserId,
    [StringLength(500)] string? Reason);
