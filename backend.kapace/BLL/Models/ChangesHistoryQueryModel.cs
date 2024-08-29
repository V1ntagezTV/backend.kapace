using backend.kapace.BLL.Enums;

namespace backend.kapace.BLL.Models;

public record ChangesHistoryQueryModel
{
    public long[] Ids { get; init; }
    public long[] TargetIds { get; init; }
    public long[] CreatedByIds { get; init; }
    public HistoryType[] HistoryTypes { get; init; }
    public int Limit { get; init; }
    public int Offset { get; init; }
}