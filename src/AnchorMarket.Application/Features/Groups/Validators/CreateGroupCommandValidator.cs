using AnchorMarket.Application.Features.Groups.Commands;
using FluentValidation;

namespace AnchorMarket.Application.Features.Groups.Validators;

/// <summary>Validates <see cref="CreateGroupCommand"/>.</summary>
public class CreateGroupCommandValidator : AbstractValidator<CreateGroupCommand>
{
    /// <summary>Defines validation rules for creating a group.</summary>
    public CreateGroupCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(1000);
        RuleFor(x => x.OwnerId).NotEmpty();
    }
}
