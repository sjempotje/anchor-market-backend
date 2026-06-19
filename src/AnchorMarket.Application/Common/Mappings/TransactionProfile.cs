using AnchorMarket.Application.Features.Wallets.DTOs;
using AnchorMarket.Domain.Entities;
using AutoMapper;

namespace AnchorMarket.Application.Common.Mappings;

/// <summary>AutoMapper profile for mapping between <see cref="Transaction"/> and <see cref="TransactionDto"/>.</summary>
public class TransactionProfile : Profile
{
    /// <summary>Configures entity-to-DTO mappings.</summary>
    public TransactionProfile()
    {
        CreateMap<Transaction, TransactionDto>();
    }
}
