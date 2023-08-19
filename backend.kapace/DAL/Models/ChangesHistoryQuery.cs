using backend.kapace.BLL.Enums;

namespace backend.kapace.DAL.Models;

internal class ChangesHistoryQuery
{
    public long[] Ids { get; init; }
    public long[] TargetIds { get; init; }
    public HistoryType[] HistoryTypes { get; init; }
}