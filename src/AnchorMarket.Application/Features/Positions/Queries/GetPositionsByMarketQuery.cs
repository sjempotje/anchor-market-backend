using AnchorMarket.Application.Features.Positions.DTOs;
using MediatR;

namespace AnchorMarket.Application.Features.Positions.Queries;

public record GetPositionsByMarketQuery(Guid MarketId, Guid UserId) : IRequest<List<PositionDto>>;

public class GetPositionsByMarketQueryHandler : IRequestHandler<GetPositionsByMarketQuery, List<PositionDto>>
{
    public Task<List<PositionDto>> Handle(GetPositionsByMarketQuery request, CancellationToken cancellationToken)
        => throw new NotImplementedException();
}
