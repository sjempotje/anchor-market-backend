namespace AnchorMarket.Application.Features.Products.DTOs;

public record ProductDto(
    Guid Id,
    string Name,
    string Description,
    decimal Price,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);
