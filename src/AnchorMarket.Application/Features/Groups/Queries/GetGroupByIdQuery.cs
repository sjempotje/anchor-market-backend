using AnchorMarket.Application.Common.Interfaces;
using AnchorMarket.Application.Common.Queries;
using AnchorMarket.Application.Features.Groups.DTOs;
using AnchorMarket.Domain.Entities;
using AutoMapper;
using MediatR;

namespace AnchorMarket.Application.Features.Groups.Queries;

public record GetGroupByIdQuery(Guid Id) : IRequest<GroupDto?>, IGetByIdQuery;

public class GetGroupByIdQueryHandler(IApplicationDbContext context, IMapper mapper)
    : GetByIdQueryHandler<Group, GetGroupByIdQuery, GroupDto>(context, mapper);
