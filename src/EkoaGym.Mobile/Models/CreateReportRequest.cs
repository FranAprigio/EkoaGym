namespace EkoaGym.Mobile.Models;

public record CreateReportRequest(Guid TargetUserId, string? Reason);
