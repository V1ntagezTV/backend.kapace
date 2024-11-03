using backend.kapace.BLL.Enums;

namespace backend.kapace.Models.Response;

public record V1GetByQueryResponse 
{
    public required IReadOnlyCollection<V1GetByQueryContent> Content { get; init; }

    public record V1GetByQueryContent(
        QueryContent Content,
        V1GetByQueryTranslation[] Translations,
        V1GetByQueryEpisode[] Episodes,
        V1GetByQueryGenre[] Genres);

    public record V1GetByQueryEpisode
    {
        public required long Id {get; set; }
        public required long ContentId {get; set; }
        public required string Title {get; set; }
        public required long? Image {get; set; }
        public required int Number {get; set; }
    }
    
    public record V1GetByQueryTranslation
    {
        public required long Id { get; init; }
        public required long EpisodeId { get; init; }
        public required long ContentId { get; init; }
        public required Language Language { get; init; }
        public required string Link { get; init; }
        public required TranslationType TranslationType { get; init; }
        public required DateTimeOffset CreatedAt { get; init; }
        public required long CreatedBy { get; init; }
        public required long? TranslatorId { get; init; }
        public required string? TranslatorName { get; init; }
        public required string? TranslatorLink { get; init; }
    }

    public record V1GetByQueryGenre
    {
        public long ContentId { get; set; }
        public long GenreId { get; set; }
        public string Name { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public long? CreatedBy { get; set; }
    }
}


