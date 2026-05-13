using AnchorMarket.Application.Common.Interfaces;
using AnchorMarket.Application.Common.Queries;
using AnchorMarket.Application.Features.Markets.DTOs;
using AnchorMarket.Domain.Entities;
using AutoMapper;
using MediatR;

namespace AnchorMarket.Application.Features.GroupMarkets.Queries;

public record GetGroupMarketByIdQuery(Guid Id) : IRequest<MarketDto?>, IGetByIdQuery;

public class GetGroupMarketByIdQueryHandler(IApplicationDbContext context, IMapper mapper)
    : GetByIdQueryHandler<Market, GetGroupMarketByIdQuery, MarketDto>(context, mapper);
