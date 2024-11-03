using backend.kapace.BLL.Enums;
using backend.Models.Enums;

namespace backend.kapace.BLL.Models.HistoryChanges;

public class HistoryUnit
{
    public long Id { get; init; }
    public long? TargetId { get; init; }
    public HistoryType HistoryType { get; init; }
    public JsonChanges Changes { get; init; }
    public long CreatedBy { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public long? ApprovedBy { get; init; }
    public DateTimeOffset? ApprovedAt { get; init; }
    
    
    public record JsonChanges { }
    
    public record JsonContentChanges : JsonChanges
    {
        public long? ImageId { get; set; }
        public string? Title { get; init; }
        public string? EngTitle { get; init; }
        public string? OriginTitle { get; init; }
        public string? Description { get; init; }
        public string? Channel { get; init; }
        public ContentStatus? Status { get; init; }
        public Country? Country { get; init; }
        public ContentType? ContentType { get; init; }
        public string[]? Genres { get; init; }
        public int? Duration { get; init; }
        public DateTimeOffset? ReleasedAt { get; init; }
        public int? PlannedSeries { get; init; }
        public int? MinAge { get; init; }
    }

    public record JsonEpisodeChanges : JsonChanges
    {
        public long? ContentId { get; init; }
        public long? EpisodeId { get; init; }
        public long? TranslatorId { get; init; }

        // Episode fields
        public int Number { get; init; }
        public long? ImageId { get; init; }
        public string? Title { get; init; }

        // Translation fields
        public TranslationType? TranslationType { get; init; }
        public string? VideoScript { get; set; }
        public Language? Language { get; init; }
        public int? Quality { get; init; }
    }
}