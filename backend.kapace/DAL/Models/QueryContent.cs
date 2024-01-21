using backend.kapace.BLL.Enums;
using backend.Models.Enums;

namespace backend.kapace.DAL.Models;

public class QueryContent
{
    public long[]? Ids { get; init; }
    public int[]? Countries { get; init; }
    public int[]? Statuses { get; init; }
    public int[]? Types { get; init; }
    public long Limit { get; init; }

    public long Offset { get; init; }

}