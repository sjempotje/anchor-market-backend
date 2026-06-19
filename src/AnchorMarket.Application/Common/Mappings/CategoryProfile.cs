using AnchorMarket.Application.Features.Categories.DTOs;
using AnchorMarket.Domain.Entities;
using AutoMapper;

namespace AnchorMarket.Application.Common.Mappings;

/// <summary>AutoMapper profile for mapping between <see cref="Category"/> and <see cref="CategoryDto"/>.</summary>
public class CategoryProfile : Profile
{
    /// <summary>Configures entity-to-DTO mappings.</summary>
    public CategoryProfile()
    {
        CreateMap<Category, CategoryDto>();
    }
}
