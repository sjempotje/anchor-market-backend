using AnchorMarket.Application.Features.Markets.Commands;
using FluentValidation;

namespace AnchorMarket.Application.Features.Markets.Validators;

/// <summary>Validates <see cref="CreateMarketCommand"/>.</summary>
public class CreateMarketCommandValidator : AbstractValidator<CreateMarketCommand>
{
    /// <summary>Defines validation rules for creating a market.</summary>
    public CreateMarketCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(300);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(2000);
        RuleFor(x => x.ResolutionDeadline).GreaterThan(DateTimeOffset.UtcNow);
        RuleFor(x => x.OutcomeTitles).NotEmpty();
        RuleFor(x => x.CreatorId).NotEmpty();
    }
}
