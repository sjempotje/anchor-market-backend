using AnchorMarket.Domain.Entities;

namespace AnchorMarket.Application.Common.Interfaces;

/// <summary>
/// Fetches and parses data from a single external source. Implementations are registered by
/// <see cref="AdapterType"/> and resolved per market via <see cref="IFeedAdapterFactory"/>.
/// </summary>
public interface IExternalFeedAdapter
{
    /// <summary>Gets the discriminator that maps a feed registration to this adapter (e.g. "BinanceCrypto").</summary>
    string AdapterType { get; }

    /// <summary>Fetches the latest value for the given feed registration.</summary>
    /// <param name="registration">The feed configuration describing what to fetch and how to parse it.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The fetch result, including the raw response and any parsed value.</returns>
    Task<FeedFetchResult> FetchAsync(ExternalFeedRegistration registration, CancellationToken cancellationToken);

    // Future extension point for push-based feeds:
    // Task<Stream> GetLiveStreamAsync(ExternalFeedRegistration registration, CancellationToken cancellationToken);
}
