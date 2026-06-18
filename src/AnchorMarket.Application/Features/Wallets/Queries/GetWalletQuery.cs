using AnchorMarket.Application.Common.Exceptions;
using AnchorMarket.Application.Common.Interfaces;
using AnchorMarket.Application.Features.Wallets.DTOs;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AnchorMarket.Application.Features.Wallets.Queries;

public record GetWalletQuery(Guid UserId, Guid CallerId) : IRequest<WalletDto?>;

public class GetWalletQueryHandler(IApplicationDbContext context, IMapper mapper)
    : IRequestHandler<GetWalletQuery, WalletDto?>
{
    public async Task<WalletDto?> Handle(GetWalletQuery request, CancellationToken cancellationToken)
    {
        if (request.UserId != request.CallerId) throw new ForbiddenException("You do not own this wallet.");

        return await context.Wallets
            .Where(w => w.UserId == request.UserId)
            .ProjectTo<WalletDto>(mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
