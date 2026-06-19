using AnchorMarket.Application.Features.Wallets.DTOs;
using AnchorMarket.Domain.Entities;
using AutoMapper;

namespace AnchorMarket.Application.Common.Mappings;

/// <summary>AutoMapper profile for mapping between <see cref="Wallet"/> and <see cref="WalletDto"/>.</summary>
public class WalletProfile : Profile
{
    /// <summary>Configures entity-to-DTO mappings.</summary>
    public WalletProfile()
    {
        CreateMap<Wallet, WalletDto>();
    }
}
