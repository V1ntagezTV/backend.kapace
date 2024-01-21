using backend.kapace.BLL.Enums;
using backend.Models.Enums;

namespace backend.kapace.BLL.Models;

public class SearchFilters
{
    public Country[] Countries { get; init; } = Array.Empty<Country>();
    public ContentType[] ContentTypes { get; init; } = Array.Empty<ContentType>();
    public ContentStatus[] ContentStatuses { get; init; } = Array.Empty<ContentStatus>();
    public long[] GenreIds { get; init; } = Array.Empty<long>();
    public long[] ContentIds { get; init; } = Array.Empty<long>();
}