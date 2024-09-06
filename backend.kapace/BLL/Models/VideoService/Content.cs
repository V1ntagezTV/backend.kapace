using backend.kapace.BLL.Enums;
using backend.Models.Enums;

namespace backend.kapace.BLL.Models.VideoService;

public record Content
{
    private Content() { }
    
    public long Id { get; private init; }
    public string Title { get; private init; }
    public string EngTitle { get;private init; }
    public string OriginTitle { get; private init; }
    public string Description { get; private init; }
    public ContentType ContentType { get; private init; }
    public ContentStatus Status { get; private init; }
    public long ImageId { get; private init; }
    public decimal ImportStars { get; private init; }
    public int OutSeries { get; private init; }
    public int PlannedSeries { get; private init; }
    public int Views { get; private init; }
    public Country Country { get; private init; }
    public DateTimeOffset? ReleasedAt { get; private init; }
    public DateTimeOffset CreatedAt { get; private init; }
    public DateTimeOffset LastUpdateAt { get; private init; }
    public int MinAge { get; private init; }
    public int? Duration { get; private init; }
    public string? Channel { get; private init; }

    public static Content ToBllContent(DAL.Models.Content content)
    {
        return new Content
        {
            Id = content.Id,
            Title = content.Title,
            EngTitle = content.EngTitle,
            OriginTitle = content.OriginTitle,
            Description = content.Description,
            ContentType = (ContentType)content.Type,
            Status = (ContentStatus)content.Status,
            ImageId = content.ImageId,
            ImportStars = content.ImportStars,
            OutSeries = content.OutSeries,
            PlannedSeries = content.PlannedSeries,
            Views = content.Views,
            Country = (Country)content.Country,
            ReleasedAt = content.ReleasedAt,
            CreatedAt = content.CreatedAt,
            LastUpdateAt = content.LastUpdateAt,
            MinAge = content.MinAgeLimit,
            Duration = content.Duration,
            Channel = content.Channel
        };
    } 
};