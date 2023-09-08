namespace backend.kapace.DAL.Repository;

public class InsertContentQuery
{
    public long Id { get; init; }
    public long ImageId { get; init; }
    public string Title { get; init; }
    public string Description { get; init; }
    public int ContentType { get; init; }
    public int Country { get; init; }
    public string? OriginTitle { get; init; }
    public string? EngTitle { get; init; }
    public int? Status { get; init; }
    public string? Channel { get; init; }
    public int? MinAge { get; init; }
    public int? Duration { get; init; }
    public int? PlannedSeries { get; init; }
    public double ImportStars { get; init; }
    public int OutSeries { get; init; }
    public int Views { get; init; }
    public DateTimeOffset? ReleasedAt { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset LastUpdateAt { get; init; }
}