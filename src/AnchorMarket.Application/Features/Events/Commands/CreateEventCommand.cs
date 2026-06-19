using AnchorMarket.Application.Common.Interfaces;
using AnchorMarket.Domain.Entities;
using MediatR;

namespace AnchorMarket.Application.Features.Events.Commands;

/// <summary>Command to create a new event.</summary>
public record CreateEventCommand(
    string Title,
    string? Description = null,
    string? Slug = null,
    DateTimeOffset? StartTime = null,
    DateTimeOffset? EndTime = null,
    Guid? CategoryId = null,
    string? ImageUrl = null,
    string? BannerUrl = null,
    string? HostCountry = null,
    string? Venue = null,
    decimal? PrizePool = null) : IRequest<Guid>;

/// <summary>Handles the creation of an event.</summary>
public class CreateEventCommandHandler(IApplicationDbContext context) : IRequestHandler<CreateEventCommand, Guid>
{
    /// <summary>Creates the event and returns its ID.</summary>
    public async Task<Guid> Handle(CreateEventCommand request, CancellationToken cancellationToken)
    {
        var ev = Event.Create(
            request.Title,
            request.Description,
            request.Slug,
            request.StartTime,
            request.EndTime,
            request.CategoryId,
            request.ImageUrl,
            request.BannerUrl);

        context.Events.Add(ev);
        await context.SaveChangesAsync(cancellationToken);
        return ev.Id;
    }
}
