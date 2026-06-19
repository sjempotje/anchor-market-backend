namespace AnchorMarket.Domain.Enums;

/// <summary>Defines the structure and rules of a prediction market.</summary>
public enum MarketType
{
    /// <summary>Two-outcome market (Yes/No).</summary>
    Binary,
    /// <summary>Market with more than two possible outcomes.</summary>
    MultiChoice,
    /// <summary>Market predicting the winner of a contest.</summary>
    Winner,
    /// <summary>Market on which team or player will win outright.</summary>
    Moneyline,
    /// <summary>Market against a point spread.</summary>
    Spread,
    /// <summary>Market with a handicap applied to one side.</summary>
    Handicap,
    /// <summary>Market on the total combined score.</summary>
    Total,
    /// <summary>Market on whether the score goes over or under a line.</summary>
    OverUnder,
    /// <summary>Market predicting the exact final score.</summary>
    CorrectScore,
    /// <summary>Proposition market on a specific event or occurrence.</summary>
    PropBet
}
