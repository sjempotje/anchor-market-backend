namespace AnchorMarket.Application.Features.Comments.DTOs;

/// <summary>Data transfer object for a comment.</summary>
public record CommentDto(
    Guid Id,
    Guid MarketId,
    Guid UserId,
    string Content,
    int Upvotes,
    Guid? ParentCommentId,
    DateTimeOffset CreatedAt);
