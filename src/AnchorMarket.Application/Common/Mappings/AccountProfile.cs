using AnchorMarket.Application.Features.Accounts.DTOs;
using AnchorMarket.Domain.Entities;
using AutoMapper;

namespace AnchorMarket.Application.Common.Mappings;

/// <summary>AutoMapper profile for mapping between <see cref="Account"/> and <see cref="AccountDto"/>.</summary>
public class AccountProfile : Profile
{
    /// <summary>Configures entity-to-DTO mappings.</summary>
    public AccountProfile()
    {
        CreateMap<Account, AccountDto>();
    }
}
