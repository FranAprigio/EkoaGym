using System.ComponentModel.DataAnnotations;

namespace EkoaGym.Application.Sessions;

public record StartSessionRequest([property: Required] Guid PlanId);
