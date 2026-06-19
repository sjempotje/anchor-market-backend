using AnchorMarket.Application.Common.Interfaces;
using AnchorMarket.Domain.Entities;
using MediatR;

namespace AnchorMarket.Application.Features.Comments.Commands;

/// <summary>Command to create a new comment on a market.</summary>
public record CreateCommentCommand(
    Guid MarketId,
    Guid UserId,
    string Content,
    Guid? ParentCommentId = null) : IRequest<Guid>;

/// <summary>Handles the creation of a comment.</summary>
public class CreateCommentCommandHandler(IApplicationDbContext context) : IRequestHandler<CreateCommentCommand, Guid>
{
    /// <summary>Creates the comment and returns its ID.</summary>
    public async Task<Guid> Handle(CreateCommentCommand request, CancellationToken cancellationToken)
    {
        var comment = Comment.Create(request.MarketId, request.UserId, request.Content, request.ParentCommentId);
        context.Comments.Add(comment);
        await context.SaveChangesAsync(cancellationToken);
        return comment.Id;
    }
}
