namespace EkoaGym.Application.Common.Exceptions;

public class SessionNotCompletedException() : Exception("The workout session must be completed before its score can be calculated.");
