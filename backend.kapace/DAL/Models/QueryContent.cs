using backend.kapace.BLL.Enums;
using backend.Models.Enums;

namespace backend.kapace.DAL.Models;

public class QueryContent
{
    public long[] Ids { get; init; } = Array.Empty<long>();
    public int[] Countries { get; set; }
    public int[] Statuses { get; set; }
    public int[] Types { get; set; }
    public long Limit { get; init; }

    public long Offset { get; init; }

}