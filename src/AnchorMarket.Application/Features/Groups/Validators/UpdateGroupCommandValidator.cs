using AnchorMarket.Application.Features.Groups.Commands;
using FluentValidation;

namespace AnchorMarket.Application.Features.Groups.Validators;

/// <summary>Validates <see cref="UpdateGroupCommand"/>.</summary>
public class UpdateGroupCommandValidator : AbstractValidator<UpdateGroupCommand>
{
    /// <summary>Defines validation rules for updating a group.</summary>
    public UpdateGroupCommandValidator()
    {
        RuleFor(x => x.GroupId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(1000);
    }
}