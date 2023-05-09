using backend.kapace.BLL.Enums;
using backend.Models.Enums;

namespace backend.kapace.BLL.Models;

public class ContentQuery
{
    public long[] Ids { get; set; } = Array.Empty<long>();
    public ContentStatus[] Statuses { get; set; } = Array.Empty<ContentStatus>();
    public ContentType[] Types { get; set; } = Array.Empty<ContentType>();
    public int Offset { get; set; }
    public int Limit { get; init; }
    public Country[] Countries { get; init; } = Array.Empty<Country>();
}