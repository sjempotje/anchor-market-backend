using AnchorMarket.Application.Features.Wallets.DTOs;
using AnchorMarket.Domain.Entities;
using AutoMapper;

namespace AnchorMarket.Application.Common.Mappings;

public class WalletProfile : Profile
{
    public WalletProfile()
    {
        CreateMap<Wallet, WalletDto>();
    }
}
