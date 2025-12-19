namespace Core.Filters;

public record ProductFilter
(
    long[] ProductIds,
    decimal? MinPrice,
    decimal? MaxPrice,
    string? NameSubstring,
    int Cursor,
    int PageSize);