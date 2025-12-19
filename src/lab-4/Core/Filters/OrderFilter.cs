using Core.Models.Enums;

namespace Core.Filters;

public record OrderFilter
(
    long[]? OrderIds,
    OrderState? OrderState,
    string? CreatedBy,
    long Cursor,
    int PageSize);