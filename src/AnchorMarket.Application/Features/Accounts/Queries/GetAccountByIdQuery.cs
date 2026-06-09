using AnchorMarket.Application.Common.Interfaces;
using AnchorMarket.Application.Features.Accounts.DTOs;
using AnchorMarket.Domain.Entities;
using MediatR;

namespace AnchorMarket.Application.Features.Accounts.Queries;

public record GetAccountByIdQuery(Guid Id) : IRequest<AccountDto?>;

public class GetAccountByIdQueryHandler : IRequestHandler<GetAccountByIdQuery, AccountDto?>
{
    private readonly IApplicationDbContext _context;

    public GetAccountByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AccountDto?> Handle(GetAccountByIdQuery request, CancellationToken cancellationToken)
    {
        var account = await _context.Set<Account>().FindAsync([request.Id], cancellationToken);

        if (account is null)
            return null;

        return new AccountDto(
            account.Id,
            account.UserId,
            account.AccountId,
            account.ProviderId,
            account.AccessToken,
            account.RefreshToken,
            account.AccessTokenExpiresAt,
            account.RefreshTokenExpiresAt,
            account.Scope,
            account.IdToken,
            account.Password,
            account.CreatedAt,
            account.UpdatedAt);
    }
}
