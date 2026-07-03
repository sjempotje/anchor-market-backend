namespace AnchorMarket.Application.Common.Queries;

/// <summary>A page of results along with the total count across all pages.</summary>
public record PagedResult<T>(IReadOnlyList<T> Items, int TotalCount, int Page, int PageSize);
