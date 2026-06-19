using AnchorMarket.Application.Common.Interfaces;
using AnchorMarket.Domain.Entities;
using MediatR;

namespace AnchorMarket.Application.Features.Categories.Commands;

/// <summary>Command to create a new category.</summary>
public record CreateCategoryCommand(
    string Name,
    string Slug,
    string? Icon = null,
    Guid? ParentCategoryId = null) : IRequest<Guid>;

/// <summary>Handles the creation of a new category.</summary>
public class CreateCategoryCommandHandler(IApplicationDbContext context) : IRequestHandler<CreateCategoryCommand, Guid>
{
    /// <summary>Creates the category and returns its ID.</summary>
    public async Task<Guid> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = Category.Create(request.Name, request.Slug, request.Icon, request.ParentCategoryId);
        context.Categories.Add(category);
        await context.SaveChangesAsync(cancellationToken);
        return category.Id;
    }
}
