using AnchorMarket.Application.Features.Markets.Commands;
using FluentValidation;

namespace AnchorMarket.Application.Features.Markets.Validators;

/// <summary>Validates <see cref="UpdateMarketCommand"/>.</summary>
public class UpdateMarketCommandValidator : AbstractValidator<UpdateMarketCommand>
{
    /// <summary>Defines validation rules for updating a market.</summary>
    public UpdateMarketCommandValidator()
    {
        RuleFor(x => x.MarketId).NotEmpty();
        RuleFor(x => x.Title).NotEmpty().MaximumLength(300);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(2000);
        RuleFor(x => x.ResolutionDeadline).GreaterThan(DateTimeOffset.UtcNow);
    }
}
