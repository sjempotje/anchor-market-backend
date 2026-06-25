using AnchorMarket.Application.Common.Interfaces;
using AnchorMarket.Application.Common.Queries;
using AnchorMarket.Application.Features.ExternalFeeds.DTOs;
using AnchorMarket.Domain.Entities;
using AutoMapper;
using MediatR;

namespace AnchorMarket.Application.Features.ExternalFeeds.Queries;

/// <summary>Query to retrieve a feed registration by its ID.</summary>
public record GetFeedByIdQuery(Guid Id) : IRequest<FeedRegistrationDto?>, IGetByIdQuery;

/// <summary>Handles retrieving a feed registration by ID.</summary>
public class GetFeedByIdQueryHandler(IApplicationDbContext context, IMapper mapper)
    : GetByIdQueryHandler<ExternalFeedRegistration, GetFeedByIdQuery, FeedRegistrationDto>(context, mapper);
