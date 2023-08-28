using backend.Models.Enums;

namespace backend.kapace.Models.Response;

public record V1QueryResponse(V1QueryResponse.Content[] Contents)
{
    public class Content
    {
        public long Id { get; init; }
        public string Title { get; init; }
        public string Description { get; init; }
        public ContentType Type { get; init; }
        public ContentStatus Status { get; init; }
        public long ImageId { get; init; }
        public decimal ImportStars { get; init; }
        public int OutSeries { get; init; }
        public int PlannedSeries { get; init; }
        public int Views { get; init; }
        public int Country { get; init; }
        public DateTimeOffset ReleasedAt { get; init; }
        public DateTimeOffset CreatedAt { get; init; }
        public DateTimeOffset LastUpdateAt { get; init; }
        public int MinAgeLimit { get; init; }
        public int? Duration { get; init; }
    }
}