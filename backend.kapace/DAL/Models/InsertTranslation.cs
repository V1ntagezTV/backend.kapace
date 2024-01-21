using backend.kapace.BLL.Enums;

namespace backend.kapace.DAL.Models;

public record InsertTranslation(
    long ContentId,
    long EpisodeId,
    long TranslatorId,
    Language Lang,
    string Link,
    TranslationType TranslationType,
    DateTimeOffset CreatedAt,
    long CreatedBy,
    int Quality
);