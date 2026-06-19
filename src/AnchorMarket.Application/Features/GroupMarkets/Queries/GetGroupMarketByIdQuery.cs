using AnchorMarket.Application.Common.Interfaces;
using AnchorMarket.Application.Common.Queries;
using AnchorMarket.Application.Features.Markets.DTOs;
using AnchorMarket.Domain.Entities;
using AutoMapper;
using MediatR;

namespace AnchorMarket.Application.Features.GroupMarkets.Queries;

/// <summary>Query to retrieve a group market by its ID.</summary>
public record GetGroupMarketByIdQuery(Guid Id) : IRequest<MarketDto?>, IGetByIdQuery;

/// <summary>Handles retrieving a group market by ID.</summary>
public class GetGroupMarketByIdQueryHandler(IApplicationDbContext context, IMapper mapper)
    : GetByIdQueryHandler<Market, GetGroupMarketByIdQuery, MarketDto>(context, mapper);
