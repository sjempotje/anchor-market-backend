using AnchorMarket.Application.Features.Users.Commands;
using FluentValidation;

namespace AnchorMarket.Application.Features.Users.Validators;

/// <summary>Validates <see cref="UpdateUserCommand"/>.</summary>
public class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{
    /// <summary>Defines validation rules for updating a user.</summary>
    public UpdateUserCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Username).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(200);
    }
}
