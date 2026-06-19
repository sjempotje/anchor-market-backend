using AnchorMarket.Application.Common.Interfaces;
using AnchorMarket.Application.Common.Queries;
using AnchorMarket.Application.Features.Markets.DTOs;
using AnchorMarket.Domain.Entities;
using AutoMapper;
using MediatR;

namespace AnchorMarket.Application.Features.Markets.Queries;

/// <summary>Query to retrieve all markets.</summary>
public record GetMarketsQuery : IRequest<List<MarketDto>>;

/// <summary>Handles retrieving all markets.</summary>
public class GetMarketsQueryHandler(IApplicationDbContext context, IMapper mapper)
    : GetAllQueryHandler<Market, GetMarketsQuery, MarketDto>(context, mapper);
