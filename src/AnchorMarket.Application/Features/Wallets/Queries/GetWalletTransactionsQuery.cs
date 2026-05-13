using AnchorMarket.Application.Common.Interfaces;
using AnchorMarket.Application.Features.Wallets.DTOs;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AnchorMarket.Application.Features.Wallets.Queries;

public record GetWalletTransactionsQuery(Guid WalletId) : IRequest<List<TransactionDto>>;

public class GetWalletTransactionsQueryHandler : IRequestHandler<GetWalletTransactionsQuery, List<TransactionDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetWalletTransactionsQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public Task<List<TransactionDto>> Handle(GetWalletTransactionsQuery request, CancellationToken cancellationToken)
        => _context.Transactions
            .Where(t => t.WalletId == request.WalletId)
            .ProjectTo<TransactionDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
}
