using AnchorMarket.Domain.Enums;

namespace AnchorMarket.Domain.Entities;

/// <summary>Reusable blueprint for generating markets automatically (e.g. Moneyline for every match).</summary>
public class MarketTemplate : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public MarketType MarketType { get; private set; }

    /// <summary>JSON array of default outcome title templates (e.g. ["{{HomeTeam}}", "{{AwayTeam}}", "Draw"]).</summary>
    public string OutcomeTitlesJson { get; private set; } = "[]";

    public Guid? SportId { get; private set; }

    public static MarketTemplate Create(string name, MarketType marketType, string outcomeTitlesJson,
        Guid? sportId = null)
    {
        return new MarketTemplate
        {
            Name = name,
            MarketType = marketType,
            OutcomeTitlesJson = outcomeTitlesJson,
            SportId = sportId
        };
    }
}
