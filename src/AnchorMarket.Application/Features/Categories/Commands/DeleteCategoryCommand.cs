using AnchorMarket.Application.Common.Exceptions;
using AnchorMarket.Application.Common.Interfaces;
using MediatR;

namespace AnchorMarket.Application.Features.Categories.Commands;

/// <summary>Command to delete a category by ID.</summary>
public record DeleteCategoryCommand(Guid Id) : IRequest;

/// <summary>Handles the deletion of a category.</summary>
public class DeleteCategoryCommandHandler(IApplicationDbContext context) : IRequestHandler<DeleteCategoryCommand>
{
    /// <summary>Deletes the category if it exists.</summary>
    public async Task Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await context.Categories.FindAsync([request.Id], cancellationToken)
            ?? throw new NotFoundException($"Category {request.Id} not found.");
        context.Categories.Remove(category);
        await context.SaveChangesAsync(cancellationToken);
    }
}
