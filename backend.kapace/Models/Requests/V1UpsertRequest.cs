using backend.kapace.BLL.Enums;
using backend.Models.Enums;

namespace backend.kapace.Models.Requests;

public sealed record V1UpsertRequest(
    long? Id,
    string Title,
    string Description,
    Country Country,
    ContentType Type,
    string Genre,
    TimeSpan Duration,
    DateTimeOffset ReleasedAt,
    int PlannedSeriesCount,
    int AgeLimit
);