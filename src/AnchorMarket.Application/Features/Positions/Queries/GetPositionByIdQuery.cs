using AnchorMarket.Application.Common.Interfaces;
using AnchorMarket.Application.Common.Queries;
using AnchorMarket.Application.Features.Positions.DTOs;
using AnchorMarket.Domain.Entities;
using AutoMapper;
using MediatR;

namespace AnchorMarket.Application.Features.Positions.Queries;

public record GetPositionByIdQuery(Guid Id) : IRequest<PositionDto?>, IGetByIdQuery;

public class GetPositionByIdQueryHandler(IApplicationDbContext context, IMapper mapper)
    : GetByIdQueryHandler<Position, GetPositionByIdQuery, PositionDto>(context, mapper);
