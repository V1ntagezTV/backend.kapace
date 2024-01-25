namespace backend.kapace.DAL.Models;

public class ContentUpdateQuery
{
    public ContentUpdateQuery(long contentId)
    {
        ContentId = contentId;
    }
    
    public long ContentId { get; }
    public long? ImageId { get; set; }
    public string? Title { get; set; }
    public string? EngTitle { get; set; }
    public string? OriginTitle { get; set; }
    public string? Description { get; set; }
    public int? Country { get; set; }
    public int? ContentType { get; set; }
    public int? Duration { get; set; }
    public DateTimeOffset? ReleasedAt { get; set; }
    public int? PlannedSeries { get; set; }
    public int? MinAge { get; set; }
}