using AnchorMarket.Application.Features.GroupMarkets.Commands;
using FluentValidation;

namespace AnchorMarket.Application.Features.GroupMarkets.Validators;

/// <summary>Validates <see cref="CreateGroupMarketCommand"/>.</summary>
public class CreateGroupMarketCommandValidator : AbstractValidator<CreateGroupMarketCommand>
{
    /// <summary>Defines validation rules for creating a group market.</summary>
    public CreateGroupMarketCommandValidator()
    {
        RuleFor(x => x.GroupId).NotEmpty();
        RuleFor(x => x.CreatorId).NotEmpty();
        RuleFor(x => x.Title).NotEmpty().MaximumLength(300);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(2000);
        RuleFor(x => x.ResolutionDeadline).GreaterThan(DateTimeOffset.UtcNow);
        RuleFor(x => x.OutcomeTitles).NotEmpty();
        RuleFor(x => x.ResolverId).NotEmpty();
    }
}
