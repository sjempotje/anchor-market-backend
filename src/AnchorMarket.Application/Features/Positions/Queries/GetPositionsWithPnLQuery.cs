using AnchorMarket.Application.Common.Interfaces;
using AnchorMarket.Application.Features.Positions.DTOs;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace AnchorMarket.Application.Features.Positions.Queries;

/// <summary>Query to retrieve all positions for a user with calculated PnL.</summary>
public record GetPositionsWithPnLQuery(Guid UserId) : IRequest<IReadOnlyList<PositionWithPnLDto>>;

/// <summary>Handles retrieving positions with PnL calculations.</summary>
public class GetPositionsWithPnLQueryHandler : IRequestHandler<GetPositionsWithPnLQuery, IReadOnlyList<PositionWithPnLDto>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IMapper _mapper;

    public GetPositionsWithPnLQueryHandler(
        IApplicationDbContext dbContext,
        IMapper mapper)
    {
        _dbContext = dbContext;
        _mapper = mapper;
    }

    /// <summary>Returns positions with calculated unrealized PnL and ROI.</summary>
    public async Task<IReadOnlyList<PositionWithPnLDto>> Handle(
        GetPositionsWithPnLQuery request, 
        CancellationToken cancellationToken)
    {
        var positions = await _dbContext.Positions
            .Include(p => p.Outcome)
            .ThenInclude(o => o.Market)
            .Where(p => p.UserId == request.UserId)
            .ToListAsync(cancellationToken);

        var dtos = positions.Select(p => new PositionWithPnLDto(
            p.Id,
            p.UserId,
            p.Outcome.MarketId,
            p.Outcome.Market.Title,
            p.OutcomeId,
            p.Outcome.Title,
            p.Amount,
            p.Shares,
            p.EntryPrice,
            p.FairValueAtEntry,
            p.CurrentFairValue,
            p.CalculateUnrealizedPnL(),
            p.CalculateReturnOnInvestment(),
            p.CurrentFairValue,
            p.CreatedAt,
            p.UpdatedAt)).ToList();

        return dtos.AsReadOnly();
    }
}
