using AnchorMarket.Application.Common.Interfaces;
using AnchorMarket.Application.Common.Queries;
using AnchorMarket.Application.Features.Markets.DTOs;
using AnchorMarket.Domain.Entities;
using AutoMapper;
using MediatR;

namespace AnchorMarket.Application.Features.Markets.Queries;

public record GetMarketByIdQuery(Guid Id) : IRequest<MarketDto?>, IGetByIdQuery;

public class GetMarketByIdQueryHandler(IApplicationDbContext context, IMapper mapper)
    : GetByIdQueryHandler<Market, GetMarketByIdQuery, MarketDto>(context, mapper);
