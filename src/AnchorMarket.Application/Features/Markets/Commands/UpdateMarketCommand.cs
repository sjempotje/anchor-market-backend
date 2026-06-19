using AnchorMarket.Application.Common.Exceptions;
using AnchorMarket.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using MediatR;

namespace AnchorMarket.Application.Features.Markets.Commands;

/// <summary>Command to update an existing market.</summary>
public record UpdateMarketCommand(
    Guid MarketId,
    string Title,
    string Description,
    DateTimeOffset ResolutionDeadline,
    string? ImageUrl = null,
    string? BannerUrl = null,
    string? Thumbnail = null,
    string? Slug = null,
    string? ResolutionSource = null,
    string? ResolutionNotes = null) : IRequest;

/// <summary>Handles updating a market.</summary>
public class UpdateMarketCommandHandler(IApplicationDbContext context) : IRequestHandler<UpdateMarketCommand>
{
    /// <summary>Updates the market's details if it exists.</summary>
    public async Task Handle(UpdateMarketCommand request, CancellationToken cancellationToken)
    {
        var market = await context.Markets.FindAsync([request.MarketId], cancellationToken)
            ?? throw new NotFoundException($"Market with ID {request.MarketId} not found.");

        market.Update(request.Title, request.Description, request.ResolutionDeadline);
        market.SetImages(request.ImageUrl, request.BannerUrl, request.Thumbnail);
        market.SetResolutionSource(request.ResolutionSource, request.ResolutionNotes);
        await context.SaveChangesAsync(cancellationToken);
    }
}
