using AnchorMarket.Application.Common.Interfaces;
using AnchorMarket.Application.Common.Queries;
using AnchorMarket.Application.Features.Positions.DTOs;
using AnchorMarket.Domain.Entities;
using AutoMapper;
using MediatR;

namespace AnchorMarket.Application.Features.Positions.Queries;

public record GetPositionsQuery : IRequest<List<PositionDto>>;

public class GetPositionsQueryHandler(IApplicationDbContext context, IMapper mapper)
    : GetAllQueryHandler<Position, GetPositionsQuery, PositionDto>(context, mapper);
