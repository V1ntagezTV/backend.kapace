using backend.kapace.BLL.Enums;
using backend.Models.Enums;

namespace backend.kapace.DAL.Models;

public class ChangesHistoryQuery
{
    public long[] Ids { get; init; }
    public long[] TargetIds { get; init; }
    public HistoryType[] HistoryTypes { get; init; }
    public long[] CreatedByIds { get; init; }
    public bool? Approved { get; init; }
    public HistoryChangesOrderType? OrderBy { get; set; }
    public int Limit { get; init; }
    public int Offset { get; init; }

}