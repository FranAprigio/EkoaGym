namespace EkoaGym.Application.Common.Exceptions;

public class SessionAlreadyCompletedException() : Exception("This session has already been completed and is read-only.");
