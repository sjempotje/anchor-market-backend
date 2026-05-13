using AnchorMarket.Application.Features.Positions.Commands;
using FluentValidation;

namespace AnchorMarket.Application.Features.Positions.Validators;

public class PlacePositionCommandValidator : AbstractValidator<PlacePositionCommand>
{
    public PlacePositionCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.OutcomeId).NotEmpty();
        RuleFor(x => x.Amount).GreaterThan(0);
    }
}
