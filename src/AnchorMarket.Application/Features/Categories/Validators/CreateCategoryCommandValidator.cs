using AnchorMarket.Application.Features.Categories.Commands;
using FluentValidation;

namespace AnchorMarket.Application.Features.Categories.Validators;

/// <summary>Validates <see cref="CreateCategoryCommand"/>.</summary>
public class CreateCategoryCommandValidator : AbstractValidator<CreateCategoryCommand>
{
    /// <summary>Defines validation rules for creating a category.</summary>
    public CreateCategoryCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Slug).NotEmpty().MaximumLength(100).Matches("^[a-z0-9-]+$");
    }
}
