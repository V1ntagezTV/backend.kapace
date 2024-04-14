namespace backend.kapace.Models.Requests.EpisodeTranslation;

public record EpisodeTranslationQueryRequest(
    long[]? EpisodeTranslationIds,
    long[]? TranslatorIds,
    long[]? ContentIds,
    long[]? EpisodeIds,
    int Limit,
    int Offset);