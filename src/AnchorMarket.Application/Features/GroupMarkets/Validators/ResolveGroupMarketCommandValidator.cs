using AnchorMarket.Application.Features.GroupMarkets.Commands;
using FluentValidation;

namespace AnchorMarket.Application.Features.GroupMarkets.Validators;

/// <summary>Validates <see cref="ResolveGroupMarketCommand"/>.</summary>
public class ResolveGroupMarketCommandValidator : AbstractValidator<ResolveGroupMarketCommand>
{
    /// <summary>Defines validation rules for resolving a group market.</summary>
    public ResolveGroupMarketCommandValidator()
    {
        RuleFor(x => x.MarketId).NotEmpty();
        RuleFor(x => x.WinningOutcomeId).NotEmpty();
        RuleFor(x => x.ResolverId).NotEmpty();
    }
}
