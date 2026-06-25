namespace AnchorMarket.Application.Common.Interfaces;

/// <summary>Resolves the <see cref="IExternalFeedAdapter"/> registered for a given adapter type.</summary>
public interface IFeedAdapterFactory
{
    /// <summary>Returns whether an adapter is registered for the given type.</summary>
    /// <param name="adapterType">The adapter discriminator.</param>
    bool Supports(string adapterType);

    /// <summary>Resolves the adapter for the given type.</summary>
    /// <param name="adapterType">The adapter discriminator.</param>
    /// <returns>The matching adapter.</returns>
    /// <exception cref="System.NotSupportedException">Thrown when no adapter is registered for the type.</exception>
    IExternalFeedAdapter Resolve(string adapterType);
}
