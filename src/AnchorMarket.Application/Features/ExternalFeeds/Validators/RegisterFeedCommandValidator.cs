using AnchorMarket.Application.Common.Interfaces;
using AnchorMarket.Application.Features.ExternalFeeds.Commands;
using FluentValidation;

namespace AnchorMarket.Application.Features.ExternalFeeds.Validators;

/// <summary>Validates <see cref="RegisterFeedCommand"/>.</summary>
public class RegisterFeedCommandValidator : AbstractValidator<RegisterFeedCommand>
{
    /// <summary>Defines validation rules for registering a feed.</summary>
    /// <param name="adapterFactory">Used to confirm the requested adapter type is registered.</param>
    public RegisterFeedCommandValidator(IFeedAdapterFactory adapterFactory)
    {
        RuleFor(x => x.MarketId).NotEmpty();

        RuleFor(x => x.AdapterType)
            .NotEmpty()
            .MaximumLength(100)
            .Must(adapterFactory.Supports)
            .WithMessage("No feed adapter is registered for adapter type '{PropertyValue}'.");

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
