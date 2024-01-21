namespace backend.kapace.DAL.Models;

public class InsertContentQuery
{
    public long Id { get; set; }
    public long ImageId { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public int ContentType { get; set; }
    public int Country { get; set; }
    public string? OriginTitle { get; set; }
    public string? EngTitle { get; set; }
    public int? Status { get; set; }
    public string? Channel { get; set; }
    public int? MinAge { get; set; }
    public int? Duration { get; set; }
    public int? PlannedSeries { get; set; }
    public double ImportStars { get; set; }
    public int OutSeries { get; set; }
    public int Views { get; set; }
    public DateTimeOffset? ReleasedAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset LastUpdateAt { get; set; }
}