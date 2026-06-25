using AnchorMarket.Application.Features.ExternalFeeds.Commands;
using FluentValidation;

namespace AnchorMarket.Application.Features.ExternalFeeds.Validators;

/// <summary>Validates <see cref="UpdateFeedCommand"/>.</summary>
public class UpdateFeedCommandValidator : AbstractValidator<UpdateFeedCommand>
{
    /// <summary>Defines validation rules for updating a feed.</summary>
    public UpdateFeedCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();

        RuleFor(x => x.Config)
            .NotNull()
            .Must(FeedConfigRules.BeValidJson)
            .WithMessage("Config must be a valid JSON document.");

        RuleFor(x => x.PollingIntervalMs).GreaterThanOrEqualTo(100);
        RuleFor(x => x.TimeoutMs).InclusiveBetween(100, 60000);
        RuleFor(x => x.ApiUrl).MaximumLength(2000);
        RuleFor(x => x.AuthToken).MaximumLength(2000);
        RuleFor(x => x.ResolutionGranularitySeconds).GreaterThanOrEqualTo(1);
    }
}
