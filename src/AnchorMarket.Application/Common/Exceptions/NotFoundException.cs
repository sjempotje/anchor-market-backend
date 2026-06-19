namespace AnchorMarket.Application.Common.Exceptions;

/// <summary>Exception thrown when a requested resource was not found.</summary>
/// <param name="message">The error message describing the missing resource.</param>
public class NotFoundException(string message) : Exception(message);
