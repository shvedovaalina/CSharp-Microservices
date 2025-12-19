using Core.Models.Enums;

namespace Core.Filters;

public record OrderHistoryFilter
(
    long[]? OrderIds,
    OrderHistoryItemKind? Kind,
    long Cursor,
    int PageSize);