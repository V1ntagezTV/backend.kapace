using backend.kapace.BLL.Enums;
using backend.Models.Enums;

namespace backend.kapace.Models.Response;

public record V1GetByQueryResponse 
{
    public required IReadOnlyCollection<V1GetByQueryContent> Content { get; init; }
    
    public record V1GetByQueryContent
    {
        public required long Id { get; init; }
        public required string Title { get; init; }
        public required string EngTitle { get; init; }
        public required string OriginTitle { get; init; }
        public required string Description { get; init; }
        public required ContentType Type { get; init; }
        public required ContentStatus Status { get; init; }
        public required string Image { get; init; }
        public required decimal ImportStars { get; init; }
        public required int OutSeries { get; init; }
        public required int PlannedSeries { get; init; }
        public required int Views { get; init; }
        public required Country Country { get; init; }
        public required DateTimeOffset ReleasedAt { get; init; }
        public required DateTimeOffset CreatedAt { get; init; }
        public required DateTimeOffset LastUpdateAt { get; init; }
        public required int MinAgeLimit { get; init; }
        public required int? Duration { get; init; }

        public required V1GetByQueryTranslation[] Translations { get; init; } = Array.Empty<V1GetByQueryTranslation>();
        public required V1GetByQueryEpisode[] Episodes { get; init; } = Array.Empty<V1GetByQueryEpisode>();
        public required V1GetByQueryGenre[] Genres { get; init; } = Array.Empty<V1GetByQueryGenre>();
    }
    
    public record V1GetByQueryEpisode
    {
        public required long Id {get; set; }
        public required long ContentId {get; set; }
        public required string Title {get; set; }
        public required string Image {get; set; }
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


