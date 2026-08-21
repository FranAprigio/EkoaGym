namespace EkoaGym.Application.Common.Exceptions;

public class EmailAlreadyInUseException(string email) : Exception($"Email '{email}' is already registered.");
