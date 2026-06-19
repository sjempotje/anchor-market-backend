namespace AnchorMarket.Application.Common.Exceptions;

/// <summary>Exception thrown when the user is not authorized to perform the requested operation.</summary>
/// <param name="message">The error message describing the forbidden access.</param>
public class ForbiddenException(string message) : Exception(message);
