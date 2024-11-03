using backend.kapace.BLL.Enums;
using backend.Models.Enums;

namespace backend.kapace.Models.Requests;

public record V1GetChangesComparisonsRequest(
    long[]? Ids,
    long[]? TargetIds,
    long[]? CreatedByIds,
    HistoryType[]? HistoryTypes,
    bool? Approved,
    HistoryChangesOrderType? OrderBy,
    int Limit,
    int Offset);