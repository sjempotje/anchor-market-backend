using AnchorMarket.Application.Common.Exceptions;
using AnchorMarket.Application.Common.Interfaces;
using MediatR;

namespace AnchorMarket.Application.Features.ExternalFeeds.Commands;

/// <summary>Command to update an existing external feed registration.</summary>
public record UpdateFeedCommand(
    Guid Id,
    string Config,
    int PollingIntervalMs,
    int TimeoutMs,
    string? ApiUrl,
    string? AuthToken,
    int ResolutionGranularitySeconds,
    bool IsActive) : IRequest;

/// <summary>Handles updates to a feed registration.</summary>
public class UpdateFeedCommandHandler(IApplicationDbContext context) : IRequestHandler<UpdateFeedCommand>
{
    /// <summary>Applies the update to the feed registration if it exists.</summary>
    public async Task Handle(UpdateFeedCommand request, CancellationToken cancellationToken)
    {
        var registration = await context.ExternalFeedRegistrations.FindAsync([request.Id], cancellationToken)
            ?? throw new NotFoundException($"Feed registration {request.Id} not found.");

        registration.Update(
            request.Config,
            request.PollingIntervalMs,
            request.TimeoutMs,
            request.ApiUrl,
            request.AuthToken,
            request.ResolutionGranularitySeconds);
        registration.SetActive(request.IsActive);

        await context.SaveChangesAsync(cancellationToken);
    }
}
