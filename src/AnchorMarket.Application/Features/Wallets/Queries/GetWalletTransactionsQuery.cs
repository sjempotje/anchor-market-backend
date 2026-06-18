using AnchorMarket.Application.Common.Exceptions;
using AnchorMarket.Application.Common.Interfaces;
using AnchorMarket.Application.Features.Wallets.DTOs;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AnchorMarket.Application.Features.Wallets.Queries;

public record GetWalletTransactionsQuery(Guid UserId, Guid CallerId) : IRequest<List<TransactionDto>>;

public class GetWalletTransactionsQueryHandler(IApplicationDbContext context, IMapper mapper)
    : IRequestHandler<GetWalletTransactionsQuery, List<TransactionDto>>
{
    public async Task<List<TransactionDto>> Handle(GetWalletTransactionsQuery request, CancellationToken cancellationToken)
    {
        if (request.UserId != request.CallerId) throw new ForbiddenException("You do not own this wallet.");

        var walletId = await context.Wallets
            .Where(w => w.UserId == request.UserId)
            .Select(w => (Guid?)w.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (walletId is null) return [];

        return await context.Transactions
            .Where(t => t.WalletId == walletId)
            .ProjectTo<TransactionDto>(mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }
}
