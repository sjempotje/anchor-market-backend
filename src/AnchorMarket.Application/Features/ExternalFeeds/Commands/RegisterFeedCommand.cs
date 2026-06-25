using AnchorMarket.Application.Common.Exceptions;
using AnchorMarket.Application.Common.Interfaces;
using AnchorMarket.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AnchorMarket.Application.Features.ExternalFeeds.Commands;

/// <summary>Command to register an external data feed for a market.</summary>
public record RegisterFeedCommand(
    Guid MarketId,
    string AdapterType,
    string Config = "{}",
    int PollingIntervalMs = 1000,
    int TimeoutMs = 3000,
    string? ApiUrl = null,
    string? AuthToken = null,
    int ResolutionGranularitySeconds = 5) : IRequest<Guid>;

/// <summary>Handles registration of an external feed.</summary>
public class RegisterFeedCommandHandler(IApplicationDbContext context) : IRequestHandler<RegisterFeedCommand, Guid>
{
    /// <summary>Creates the feed registration and returns its ID.</summary>
    public async Task<Guid> Handle(RegisterFeedCommand request, CancellationToken cancellationToken)
    {
        var marketExists = await context.Markets
            .AnyAsync(m => m.Id == request.MarketId, cancellationToken);
        if (!marketExists)
            throw new NotFoundException($"Market {request.MarketId} not found.");

        var registration = ExternalFeedRegistration.Create(
            request.MarketId,
            request.AdapterType,
            request.Config,
            request.PollingIntervalMs,
            request.TimeoutMs,
            request.ApiUrl,
            request.AuthToken,
            request.ResolutionGranularitySeconds);

        context.ExternalFeedRegistrations.Add(registration);
        await context.SaveChangesAsync(cancellationToken);
        return registration.Id;
    }
}
