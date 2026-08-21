namespace EkoaGym.Application.Sessions;

public record SessionFilter(DateTime? From, DateTime? To, int Page, int PageSize);
