using backend.kapace.BLL.Enums;
using backend.Models.Enums;

namespace backend.kapace.Models.Requests;

public sealed record V1UpsertContentRequest(
    long? ContentId,
    V1UpsertContentRequest.V1ChangeableFields? ChangeableFields,
    long CreatedBy
)
{
    public sealed record V1ChangeableFields(
        string? Image,
        string? Title,
        string? EngTitle,
        string? OriginalTitle,
        string? Description,
        Country? Country,
        ContentType? ContentType,
        string? Genres,
        int? Duration,
        DateTimeOffset? ReleasedAt,
        int? PlannedSeries,
        int? MinAge
    );
};