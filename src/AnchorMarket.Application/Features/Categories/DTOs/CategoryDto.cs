namespace AnchorMarket.Application.Features.Categories.DTOs;

/// <summary>Data transfer object for a category.</summary>
public record CategoryDto(
    Guid Id,
    string Name,
    string Slug,
    string? Icon,
    Guid? ParentCategoryId,
    DateTimeOffset CreatedAt);
