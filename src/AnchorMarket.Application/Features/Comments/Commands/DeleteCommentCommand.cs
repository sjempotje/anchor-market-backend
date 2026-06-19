using AnchorMarket.Application.Common.Exceptions;
using AnchorMarket.Application.Common.Interfaces;
using MediatR;

namespace AnchorMarket.Application.Features.Comments.Commands;

/// <summary>Command to delete a comment.</summary>
public record DeleteCommentCommand(Guid Id, Guid CallerId) : IRequest;

/// <summary>Handles the deletion of a comment.</summary>
public class DeleteCommentCommandHandler(IApplicationDbContext context) : IRequestHandler<DeleteCommentCommand>
{
    /// <summary>Deletes the comment if owned by the caller.</summary>
    public async Task Handle(DeleteCommentCommand request, CancellationToken cancellationToken)
    {
        var comment = await context.Comments.FindAsync([request.Id], cancellationToken)
            ?? throw new NotFoundException($"Comment {request.Id} not found.");

        if (comment.UserId != request.CallerId)
            throw new ForbiddenException("You can only delete your own comments.");

        context.Comments.Remove(comment);
        await context.SaveChangesAsync(cancellationToken);
    }
}
