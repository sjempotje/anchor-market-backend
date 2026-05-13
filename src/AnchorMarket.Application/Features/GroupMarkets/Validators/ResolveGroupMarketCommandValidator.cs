using AnchorMarket.Application.Features.GroupMarkets.Commands;
using FluentValidation;

namespace AnchorMarket.Application.Features.GroupMarkets.Validators;

public class ResolveGroupMarketCommandValidator : AbstractValidator<ResolveGroupMarketCommand>
{
    public ResolveGroupMarketCommandValidator()
    {
        RuleFor(x => x.MarketId).NotEmpty();
        RuleFor(x => x.WinningOutcomeId).NotEmpty();
        RuleFor(x => x.ResolverId).NotEmpty();
    }
}
