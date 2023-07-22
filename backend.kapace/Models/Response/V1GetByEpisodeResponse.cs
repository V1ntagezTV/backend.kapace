using backend.kapace.BLL.Enums;

namespace backend.kapace.Models.Response;

public class V1GetByEpisodeResponse
{
    public required V1GetByEpisodeTranslation[] Translations { get; init; }

    public class V1GetByEpisodeTranslation 
    {
        public required long TranslationId { get; init; }
        public required long EpisodeId { get; init; }
        public required Language Language { get; init; }
        public required string Link { get; init; }
        public required TranslationType TranslationType { get; init; }
        public required DateTimeOffset CreatedAt { get; init; }
        public required long CreatedBy { get; init; }
        public required long? Quality { get; init; }
        public required long? TranslatorId { get; init; }
        public required string Translator { get; init; }
        public required string TranslatorLink { get; init; }
    } 
}