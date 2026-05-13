using AnchorMarket.Application.Features.Groups.Commands;
using FluentValidation;

namespace AnchorMarket.Application.Features.Groups.Validators;

public class CreateGroupCommandValidator : AbstractValidator<CreateGroupCommand>
{
    public CreateGroupCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(1000);
        RuleFor(x => x.OwnerId).NotEmpty();
    }
}
