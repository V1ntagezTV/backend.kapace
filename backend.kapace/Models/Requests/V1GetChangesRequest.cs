using backend.kapace.BLL.Enums;

namespace backend.kapace.Models.Requests;

public record V1GetChangesRequest(
    long[] Ids,
    long[] TargetIds,
    long[] CreatedByIds,
    HistoryType[] HistoryTypes,
    int Limit,
    int Offset);