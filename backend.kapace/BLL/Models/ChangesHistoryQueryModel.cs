using backend.kapace.BLL.Enums;
using backend.Models.Enums;

namespace backend.kapace.BLL.Models;

public record ChangesHistoryQueryModel
{
    public long[] Ids { get; init; }
    public long[] TargetIds { get; init; }
    public long[] CreatedByIds { get; init; }
    public HistoryType[] HistoryTypes { get; init; }
    public int Limit { get; init; }
    public int Offset { get; init; }
    public bool? Approved { get; init; }
    public HistoryChangesOrderType? OrderBy { get; init; }
}