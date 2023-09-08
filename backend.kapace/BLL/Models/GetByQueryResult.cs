using backend.kapace.BLL.Enums;
using backend.Models.Enums;

namespace backend.kapace.BLL.Models;

public record GetByQueryResult 
{
    public long Id { get; init; }
    public string Title { get; init; }
    public string EngTitle { get; init; }
    public string OriginTitle { get; init; }
    public string Description { get; init; }
    public ContentType Type { get; init; }
    public ContentStatus Status { get; init; }
    public long ImageId { get; init; }
    public decimal ImportStars { get; init; }
    public int OutSeries { get; init; }
    public int PlannedSeries { get; init; }
    public int Views { get; init; }
    public Country Country { get; init; }
    public DateTimeOffset? ReleasedAt { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset LastUpdateAt { get; init; }
    public int MinAgeLimit { get; init; }
    public int? Duration { get; init; }

    public GetByQueryTranslation[] Translations { get; init; } = Array.Empty<GetByQueryTranslation>();
    public GetByQueryEpisode[] Episodes { get; init; } = Array.Empty<GetByQueryEpisode>();
    public GetByQueryGenre[] Genres { get; init; } = Array.Empty<GetByQueryGenre>();

    public record GetByQueryEpisode
    {
        public required long Id {get; set; }
        public required long ContentId {get; set; }
        public required string Title {get; set; }
        public required string Image {get; set; }
        public required int Number {get; set; }
    };
    public record GetByQueryTranslation
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

    public record GetByQueryGenre()
    {
        public long ContentId {get; set;}
    
        public long GenreId {get; set;}
        
        public string Name { get; set; }
    
        public DateTimeOffset CreatedAt {get; set;}
    
        public long? CreatedBy {get; set;}
    }
}
