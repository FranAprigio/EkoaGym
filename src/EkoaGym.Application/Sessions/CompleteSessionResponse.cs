namespace EkoaGym.Application.Sessions;

public record CompleteSessionResponse(Guid Id, DateTime StartedAt, DateTime EndedAt);
