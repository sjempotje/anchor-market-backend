using AnchorMarket.Application.Features.Markets.Commands;
using FluentValidation;

namespace AnchorMarket.Application.Features.Markets.Validators;

public class CreateMarketCommandValidator : AbstractValidator<CreateMarketCommand>
{
    public CreateMarketCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(300);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(2000);
        RuleFor(x => x.ResolutionDeadline).GreaterThan(DateTimeOffset.UtcNow);
        RuleFor(x => x.OutcomeTitles).NotEmpty();
        RuleFor(x => x.CreatorId).NotEmpty();
    }
}
