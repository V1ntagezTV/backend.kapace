using backend.kapace.BLL.Enums;

namespace backend.kapace.BLL.Models;

public record Translation(
    long TranslationId,
    long EpisodeId,
    Language Language,
    string Link,
    TranslationType TranslationType,
    DateTimeOffset CreatedAt,
    long CreatedBy
)
{
    // TODO:
    public record WithTranslator(Translation Translation) : Translation(Translation);
}