namespace AnchorMarket.Application.Features.Wallets.DTOs;

/// <summary>Data transfer object for a user's wallet.</summary>
public record WalletDto(
    Guid Id,
    Guid UserId,
    decimal Balance);
