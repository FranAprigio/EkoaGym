using System.ComponentModel.DataAnnotations;

namespace EkoaGym.Application.Sessions;

public record StartSessionRequest([Required] Guid PlanId);
