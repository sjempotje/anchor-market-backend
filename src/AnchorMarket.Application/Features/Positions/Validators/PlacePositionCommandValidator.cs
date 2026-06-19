using AnchorMarket.Application.Features.Positions.Commands;
using FluentValidation;

namespace AnchorMarket.Application.Features.Positions.Validators;

/// <summary>Validates <see cref="PlacePositionCommand"/>.</summary>
public class PlacePositionCommandValidator : AbstractValidator<PlacePositionCommand>
{
    /// <summary>Defines validation rules for placing a position.</summary>
    public PlacePositionCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.OutcomeId).NotEmpty();
        RuleFor(x => x.Amount).GreaterThan(0);
    }
}
