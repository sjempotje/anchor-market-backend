namespace AnchorMarket.Api.WebSockets;

/// <summary>A subscribe/unsubscribe message sent by a client.</summary>
/// <param name="Action">"subscribe" or "unsubscribe".</param>
/// <param name="Channel">"price", "orderbook", "trades", "market", or "group-market".</param>
/// <param name="MarketId">Target market, for market/trades channels.</param>
/// <param name="OutcomeId">Target outcome, for price/orderbook channels.</param>
/// <param name="GroupId">Target group, for the group-market channel.</param>
public record SubscriptionRequest(
    string? Action,
    string? Channel,
    Guid? MarketId,
    Guid? OutcomeId,
    Guid? GroupId);

/// <summary>Maps subscription requests to internal topic keys and Redis events to client topics.</summary>
public static class RealtimeTopics
{
    /// <summary>The topic carrying price updates for an outcome.</summary>
    public static string Price(Guid outcomeId) => $"price:{outcomeId}";

    /// <summary>The topic carrying order book changes for an outcome.</summary>
    public static string OrderBook(Guid outcomeId) => $"orderbook:{outcomeId}";

    /// <summary>The topic carrying executed trades for a market.</summary>
    public static string Trades(Guid marketId) => $"trades:{marketId}";

    /// <summary>The topic carrying lifecycle events for a market.</summary>
    public static string Market(Guid marketId) => $"market:{marketId}";

    /// <summary>The topic carrying events scoped to a group.</summary>
    public static string Group(Guid groupId) => $"group:{groupId}";

    /// <summary>Resolves the topic key a subscription request refers to, or null if invalid.</summary>
    /// <param name="request">The client request.</param>
    /// <param name="requiresGroupMembership">Set when the topic requires the user to be a group member.</param>
    /// <param name="groupId">The group whose membership must be checked, when applicable.</param>
    public static string? Resolve(SubscriptionRequest request, out bool requiresGroupMembership, out Guid groupId)
    {
        requiresGroupMembership = false;
        groupId = Guid.Empty;

        return request.Channel?.ToLowerInvariant() switch
        {
            "price" when request.OutcomeId is { } o => Price(o),
            "orderbook" when request.OutcomeId is { } o => OrderBook(o),
            "trades" when request.MarketId is { } m => Trades(m),
            "market" when request.MarketId is { } m => Market(m),
            "group-market" when request.GroupId is { } g => SetGroup(g, out requiresGroupMembership, out groupId),
            _ => null
        };
    }

    private static string SetGroup(Guid g, out bool requiresMembership, out Guid groupId)
    {
        requiresMembership = true;
        groupId = g;
        return Group(g);
    }
}
