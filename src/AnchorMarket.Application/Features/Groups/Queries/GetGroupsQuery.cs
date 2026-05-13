using AnchorMarket.Application.Common.Interfaces;
using AnchorMarket.Application.Common.Queries;
using AnchorMarket.Application.Features.Groups.DTOs;
using AnchorMarket.Domain.Entities;
using AutoMapper;
using MediatR;

namespace AnchorMarket.Application.Features.Groups.Queries;

public record GetGroupsQuery : IRequest<List<GroupDto>>;

public class GetGroupsQueryHandler(IApplicationDbContext context, IMapper mapper)
    : GetAllQueryHandler<Group, GetGroupsQuery, GroupDto>(context, mapper);
