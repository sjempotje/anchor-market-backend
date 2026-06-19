using AnchorMarket.Application.Common.Exceptions;
using AnchorMarket.Application.Common.Interfaces;
using AnchorMarket.Domain.Entities;
using AnchorMarket.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using MediatR;

namespace AnchorMarket.Application.Features.GroupMarkets.Commands;

/// <summary>Command to create a market scoped to a group.</summary>
public record CreateGroupMarketCommand(
    Guid GroupId,
    Guid CreatorId,
    string Title,
    string Description,
    DateTimeOffset ResolutionDeadline,
    IReadOnlyList<string> OutcomeTitles) : IRequest<Guid>;

/// <summary>Handles creating a group market.</summary>
public class CreateGroupMarketCommandHandler : IRequestHandler<CreateGroupMarketCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public CreateGroupMarketCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>Creates the group market and returns its ID.</summary>
    public async Task<Guid> Handle(CreateGroupMarketCommand request, CancellationToken cancellationToken)
    {
        var group = await _context.Groups.FindAsync([request.GroupId], cancellationToken);

        if (group is null)
            throw new NotFoundException($"Group with ID {request.GroupId} not found.");

        var isMember = await _context.GroupMemberships
            .AnyAsync(m => m.GroupId == request.GroupId && m.UserId == request.CreatorId, cancellationToken);

        if (!isMember)
            throw new InvalidOperationException("Only group members can create group markets.");

        var market = Market.Create(
            request.Title,
            request.Description,
            request.ResolutionDeadline,
            MarketScope.Group,
            request.CreatorId,
            request.GroupId,
            request.OutcomeTitles);

        _context.Markets.Add(market);
        await _context.SaveChangesAsync(cancellationToken);
        return market.Id;
    }
}
