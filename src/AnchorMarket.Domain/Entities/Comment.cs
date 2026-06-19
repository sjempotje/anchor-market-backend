namespace AnchorMarket.Domain.Entities;

/// <summary>A user comment on a prediction market, supporting threaded replies.</summary>
public class Comment : BaseEntity
{
    /// <summary>Gets the ID of the market this comment is on.</summary>
    public Guid MarketId { get; private set; }

    /// <summary>Gets the ID of the user who wrote the comment.</summary>
    public Guid UserId { get; private set; }

    /// <summary>Gets the text content of the comment.</summary>
    public string Content { get; private set; } = string.Empty;

    /// <summary>Gets the number of upvotes this comment has received.</summary>
    public int Upvotes { get; private set; }

    /// <summary>Null for top-level comments; set for replies.</summary>
    public Guid? ParentCommentId { get; private set; }

    /// <summary>Gets the market this comment belongs to.</summary>
    public Market Market { get; private set; } = null!;

    /// <summary>Gets the parent comment if this is a reply, or null for top-level comments.</summary>
    public Comment? ParentComment { get; private set; }

    /// <summary>Gets the replies to this comment.</summary>
    public ICollection<Comment> Replies { get; private set; } = new List<Comment>();

    /// <summary>Creates a new comment on a market.</summary>
    /// <param name="marketId">The target market ID.</param>
    /// <param name="userId">The commenting user's ID.</param>
    /// <param name="content">The comment text.</param>
    /// <param name="parentCommentId">Optional parent comment ID for replies.</param>
    /// <returns>A new <see cref="Comment"/> instance.</returns>
    public static Comment Create(Guid marketId, Guid userId, string content, Guid? parentCommentId = null)
    {
        return new Comment
        {
            MarketId = marketId,
            UserId = userId,
            Content = content,
            ParentCommentId = parentCommentId
        };
    }

    /// <summary>Increments the upvote count by one.</summary>
    public void Upvote() => Upvotes++;
}
