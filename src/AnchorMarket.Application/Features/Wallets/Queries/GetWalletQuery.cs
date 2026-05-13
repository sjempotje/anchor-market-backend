using AnchorMarket.Application.Common.Interfaces;
using AnchorMarket.Application.Common.Queries;
using AnchorMarket.Application.Features.Wallets.DTOs;
using AnchorMarket.Domain.Entities;
using AutoMapper;
using MediatR;

namespace AnchorMarket.Application.Features.Wallets.Queries;

public record GetWalletQuery(Guid Id) : IRequest<WalletDto?>, IGetByIdQuery;

public class GetWalletQueryHandler(IApplicationDbContext context, IMapper mapper)
    : GetByIdQueryHandler<Wallet, GetWalletQuery, WalletDto>(context, mapper);
