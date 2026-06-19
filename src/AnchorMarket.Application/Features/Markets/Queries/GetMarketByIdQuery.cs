using AnchorMarket.Application.Common.Interfaces;
using AnchorMarket.Application.Common.Queries;
using AnchorMarket.Application.Features.Markets.DTOs;
using AnchorMarket.Domain.Entities;
using AutoMapper;
using MediatR;

namespace AnchorMarket.Application.Features.Markets.Queries;

/// <summary>Query to retrieve a market by its ID.</summary>
public record GetMarketByIdQuery(Guid Id) : IRequest<MarketDto?>, IGetByIdQuery;

/// <summary>Handles retrieving a market by ID.</summary>
public class GetMarketByIdQueryHandler(IApplicationDbContext context, IMapper mapper)
    : GetByIdQueryHandler<Market, GetMarketByIdQuery, MarketDto>(context, mapper);
