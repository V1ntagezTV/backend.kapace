using backend.kapace.BLL.Enums;

namespace backend.kapace.BLL.Models;

public record EpisodeTranslation(
    long Id,
    long EpisodeId,
    Language Language,
    string Link,
    TranslationType TranslationType,
    DateTimeOffset CreatedAt,
    long CreatedBy,
    long Quality,
    EpisodeTranslation.WithTranslator? Translator,
    EpisodeTranslation.WithEpisode? Episode
)
{
    public record WithTranslator(long TranslatorId, string Name, string Link);
    public record WithEpisode(long EpisodeId, string Title, int Number, int Views, double Stars);
}