using AnchorMarket.Application.Common.Exceptions;
using AnchorMarket.Application.Common.Interfaces;
using AnchorMarket.Application.Features.ExternalFeeds.DTOs;
using MediatR;

namespace AnchorMarket.Application.Features.ExternalFeeds.Commands;

/// <summary>Command to perform a dry-run fetch against a feed registration without persisting the result.</summary>
public record TestFeedCommand(Guid Id) : IRequest<FeedResultDto>;

/// <summary>Handles a dry-run fetch used to validate a feed's configuration.</summary>
public class TestFeedCommandHandler(IApplicationDbContext context, IFeedAdapterFactory adapterFactory)
    : IRequestHandler<TestFeedCommand, FeedResultDto>
{
    /// <summary>Resolves the adapter, fetches once, and returns the result without saving it.</summary>
    public async Task<FeedResultDto> Handle(TestFeedCommand request, CancellationToken cancellationToken)
    {
        var registration = await context.ExternalFeedRegistrations.FindAsync([request.Id], cancellationToken)
            ?? throw new NotFoundException($"Feed registration {request.Id} not found.");

        var adapter = adapterFactory.Resolve(registration.AdapterType);
        var result = await adapter.FetchAsync(registration, cancellationToken);

        return new FeedResultDto(
            Guid.Empty,
            registration.Id,
            result.RawJson,
            result.ParsedValue,
            result.Status,
            result.ErrorMessage,
            DateTimeOffset.UtcNow);
    }
}
