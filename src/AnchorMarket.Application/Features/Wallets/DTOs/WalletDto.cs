namespace AnchorMarket.Application.Features.Wallets.DTOs;

public record WalletDto(
    Guid Id,
    Guid UserId,
    decimal Balance);
