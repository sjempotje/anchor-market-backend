using AnchorMarket.Application.Common.Exceptions;
using AnchorMarket.Application.Common.Interfaces;
using MediatR;

namespace AnchorMarket.Application.Features.ExternalFeeds.Commands;

/// <summary>Command to delete a feed registration by ID.</summary>
public record DeleteFeedCommand(Guid Id) : IRequest;

/// <summary>Handles deletion of a feed registration.</summary>
public class DeleteFeedCommandHandler(IApplicationDbContext context) : IRequestHandler<DeleteFeedCommand>
{
    /// <summary>Deletes the feed registration if it exists.</summary>
    public async Task Handle(DeleteFeedCommand request, CancellationToken cancellationToken)
    {
        var registration = await context.ExternalFeedRegistrations.FindAsync([request.Id], cancellationToken)
            ?? throw new NotFoundException($"Feed registration {request.Id} not found.");
        context.ExternalFeedRegistrations.Remove(registration);
        await context.SaveChangesAsync(cancellationToken);
    }
}
