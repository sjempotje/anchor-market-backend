using AnchorMarket.Application.Common.Interfaces;
using AnchorMarket.Application.Common.Queries;
using AnchorMarket.Application.Features.Groups.DTOs;
using AnchorMarket.Domain.Entities;
using AutoMapper;
using MediatR;

namespace AnchorMarket.Application.Features.Groups.Queries;

/// <summary>Query to retrieve a group by its ID.</summary>
public record GetGroupByIdQuery(Guid Id) : IRequest<GroupDto?>, IGetByIdQuery;

/// <summary>Handles retrieving a group by ID.</summary>
public class GetGroupByIdQueryHandler(IApplicationDbContext context, IMapper mapper)
    : GetByIdQueryHandler<Group, GetGroupByIdQuery, GroupDto>(context, mapper);
