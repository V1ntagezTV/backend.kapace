using backend.kapace.BLL.Enums;
using backend.Models.Enums;

namespace backend.kapace.BLL.Models;

public class SearchFilters
{
    internal Country[] Countries { get; init; } = Array.Empty<Country>();
    internal ContentType[] ContentTypes { get; init; } = Array.Empty<ContentType>();
    internal ContentStatus[] ContentStatuses { get; init; } = Array.Empty<ContentStatus>();
    internal long[] GenreIds { get; init; } = Array.Empty<long>();

    internal long[] ContentIds { get; init; } = Array.Empty<long>();
}