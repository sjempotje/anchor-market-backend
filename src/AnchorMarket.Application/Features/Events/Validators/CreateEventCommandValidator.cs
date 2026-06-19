using AnchorMarket.Application.Features.Events.Commands;
using FluentValidation;

namespace AnchorMarket.Application.Features.Events.Validators;

/// <summary>Validates <see cref="CreateEventCommand"/>.</summary>
public class CreateEventCommandValidator : AbstractValidator<CreateEventCommand>
{
    /// <summary>Defines validation rules for creating an event.</summary>
    public CreateEventCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(300);
        RuleFor(x => x.Slug).MaximumLength(200).Matches("^[a-z0-9-]*$").When(x => x.Slug is not null);
        RuleFor(x => x.EndTime).GreaterThan(x => x.StartTime).When(x => x.StartTime.HasValue && x.EndTime.HasValue);
    }
}
