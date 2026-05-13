using AutoMapper;
using AnchorMarket.Application.Features.Products.DTOs;
using AnchorMarket.Domain.Entities;

namespace AnchorMarket.Application.Common.Mappings;

public class ProductProfile : Profile
{
    public ProductProfile()
    {
        CreateMap<Product, ProductDto>();
    }
}
