using AnchorMarket.Application.Common.Exceptions;
using AnchorMarket.Application.Common.Interfaces;
using AnchorMarket.Domain.Entities;
using AnchorMarket.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using MediatR;

namespace AnchorMarket.Application.Features.Markets.Commands;

public record CreateMarketCommand(
    string Title,
    string Description,
    DateTimeOffset ResolutionDeadline,
    MarketScope Scope,
    Guid CreatorId,
    Guid? GroupId,
    IReadOnlyList<string> OutcomeTitles) : IRequest<Guid>;

public class CreateMarketCommandHandler : IRequestHandler<CreateMarketCommand, Guid>
{
    private readonly IApplicationDbContext _context;

    public CreateMarketCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(CreateMarketCommand request, CancellationToken cancellationToken)
    {
        if (request.GroupId.HasValue && request.Scope == MarketScope.Group)
        {
            var isMember = await _context.GroupMemberships
                .AnyAsync(m => m.GroupId == request.GroupId.Value && m.UserId == request.CreatorId, cancellationToken);

            if (!isMember)
                throw new InvalidOperationException("Only group members can create group markets.");
        }

        var market = Market.Create(
            request.Title,
            request.Description,
            request.ResolutionDeadline,
            request.Scope,
            request.CreatorId,
            request.GroupId,
            request.OutcomeTitles);

        _context.Markets.Add(market);
        await _context.SaveChangesAsync(cancellationToken);
        return market.Id;
    }
}
