namespace backend.kapace.DAL.Models.ContentTranslation.Query;

public sealed class Select
{
    public long[]? EpisodeTranslationIds { get; init; }
    public long[]? TranslatorIds { get; init; }
    public long[]? ContentIds { get; init; }
    public long[]? EpisodeIds { get; init; }
    public int Limit { get; init; }
    public int Offset { get; init; }
}