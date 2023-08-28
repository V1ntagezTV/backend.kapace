using backend.kapace.BLL.Enums;

namespace backend.kapace.DAL.Models;

public class ChangesHistoryQuery
{
    public long[] Ids { get; init; }
    public long[] TargetIds { get; init; }
    public HistoryType[] HistoryTypes { get; init; }
}