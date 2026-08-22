namespace EkoaGym.Application.Common.Exceptions;

public class ReportRateLimitExceededException() : Exception("You've submitted too many reports recently. Please try again later.");
