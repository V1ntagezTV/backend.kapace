using backend.kapace.BLL.Enums;
using backend.Models.Enums;

namespace backend.kapace.BLL.Models;

public sealed record UpsertModel(
    long? Id,
    string Title,
    string Description,
    Country ContentCountry,
    ContentType ContentType,
    string Genre,
    TimeSpan Duration,
    DateTimeOffset ReleasedAt,
    int PlannedSeriesCount,
    int AgeLimit
);