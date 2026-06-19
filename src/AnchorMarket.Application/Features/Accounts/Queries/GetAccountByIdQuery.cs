using AnchorMarket.Application.Common.Exceptions;
using AnchorMarket.Application.Common.Interfaces;
using AnchorMarket.Application.Features.Accounts.DTOs;
using AnchorMarket.Domain.Entities;
using MediatR;

namespace AnchorMarket.Application.Features.Accounts.Queries;

/// <summary>Query to retrieve an account by its ID. Caller must own the account.</summary>
public record GetAccountByIdQuery(Guid Id, Guid CallerId) : IRequest<AccountDto?>;

/// <summary>Handles retrieving an account by ID.</summary>
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

        if (account.UserId != request.CallerId)
            throw new ForbiddenException("You do not have access to this account.");

        return new AccountDto(
            account.Id,
            account.UserId,
            account.AccountId,
            account.ProviderId,
            account.AccessTokenExpiresAt,
            account.RefreshTokenExpiresAt,
            account.Scope,
            account.CreatedAt,
            account.UpdatedAt);
    }
}
