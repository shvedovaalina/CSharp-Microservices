namespace Core.Filters;

public record OrderItemFilter
(
    long[] OrderIds,
    long[] ProductIds,
    bool? IsDeleted,
    long Cursor,
    int PageSize);