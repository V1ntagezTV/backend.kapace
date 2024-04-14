namespace backend.kapace.BLL.Models.EpisodeTranslations;

public class EpisodeTranslationQuery
{
    public required long[]? EpisodeTranslationIds { get; init; }
    public required long[]? TranslatorIds { get; init; }
    public required long[]? ContentIds { get; init; }
    public required long[]? EpisodeIds { get; init; }
    public required int Limit { get; init; }
    public required int Offset { get; init; }
}