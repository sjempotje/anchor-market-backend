using AnchorMarket.Application.Common.Interfaces;
using AnchorMarket.Application.Common.Queries;
using AnchorMarket.Application.Features.Markets.DTOs;
using AnchorMarket.Domain.Entities;
using AutoMapper;
using MediatR;

namespace AnchorMarket.Application.Features.Markets.Queries;

public record GetMarketsQuery : IRequest<List<MarketDto>>;

public class GetMarketsQueryHandler(IApplicationDbContext context, IMapper mapper)
    : GetAllQueryHandler<Market, GetMarketsQuery, MarketDto>(context, mapper);
