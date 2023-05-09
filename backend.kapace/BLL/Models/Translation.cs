using backend.kapace.BLL.Enums;

namespace backend.kapace.BLL.Models;

public record Translation(
    long TranslationId,
    long EpisodeId,
    long Language,
    string Link,
    TranslationType TranslationType,
    DateTimeOffset CreatedAt,
    long CreatedBy
);