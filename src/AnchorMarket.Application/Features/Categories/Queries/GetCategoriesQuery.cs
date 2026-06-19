using AnchorMarket.Application.Common.Interfaces;
using AnchorMarket.Application.Common.Queries;
using AnchorMarket.Application.Features.Categories.DTOs;
using AnchorMarket.Domain.Entities;
using AutoMapper;
using MediatR;

namespace AnchorMarket.Application.Features.Categories.Queries;

/// <summary>Query to retrieve all categories.</summary>
public record GetCategoriesQuery : IRequest<List<CategoryDto>>;

/// <summary>Handles retrieving all categories.</summary>
public class GetCategoriesQueryHandler(IApplicationDbContext context, IMapper mapper)
    : GetAllQueryHandler<Category, GetCategoriesQuery, CategoryDto>(context, mapper);
