namespace AnchorMarket.Domain.Enums;

/// <summary>Defines the execution style of an order.</summary>
public enum OrderType
{
    /// <summary>An order that executes only at a specified price or better.</summary>
    Limit,
    /// <summary>An order that executes immediately at the current market price.</summary>
    Market
}
