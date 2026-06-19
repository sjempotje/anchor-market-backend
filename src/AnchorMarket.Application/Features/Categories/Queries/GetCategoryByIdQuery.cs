using AnchorMarket.Application.Common.Interfaces;
using AnchorMarket.Application.Common.Queries;
using AnchorMarket.Application.Features.Categories.DTOs;
using AnchorMarket.Domain.Entities;
using AutoMapper;
using MediatR;

namespace AnchorMarket.Application.Features.Categories.Queries;

/// <summary>Query to retrieve a category by its ID.</summary>
public record GetCategoryByIdQuery(Guid Id) : IRequest<CategoryDto?>, IGetByIdQuery;

/// <summary>Handles retrieving a category by ID.</summary>
public class GetCategoryByIdQueryHandler(IApplicationDbContext context, IMapper mapper)
    : GetByIdQueryHandler<Category, GetCategoryByIdQuery, CategoryDto>(context, mapper);
