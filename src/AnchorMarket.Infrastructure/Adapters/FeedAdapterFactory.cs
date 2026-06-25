using AnchorMarket.Application.Common.Interfaces;

namespace AnchorMarket.Infrastructure.Adapters;

/// <summary>Resolves feed adapters by their declared <see cref="IExternalFeedAdapter.AdapterType"/>.</summary>
public class FeedAdapterFactory : IFeedAdapterFactory
{
    private readonly IReadOnlyDictionary<string, IExternalFeedAdapter> _adapters;

    /// <summary>Indexes the registered adapters by type for case-insensitive lookup.</summary>
    /// <param name="adapters">All registered feed adapters.</param>
    public FeedAdapterFactory(IEnumerable<IExternalFeedAdapter> adapters)
    {
        _adapters = adapters.ToDictionary(a => a.AdapterType, StringComparer.OrdinalIgnoreCase);
    }

    /// <inheritdoc />
    public bool Supports(string adapterType)
        => !string.IsNullOrWhiteSpace(adapterType) && _adapters.ContainsKey(adapterType);

    /// <inheritdoc />
    public IExternalFeedAdapter Resolve(string adapterType)
        => _adapters.TryGetValue(adapterType ?? string.Empty, out var adapter)
            ? adapter
            : throw new NotSupportedException($"No feed adapter is registered for adapter type '{adapterType}'.");
}
