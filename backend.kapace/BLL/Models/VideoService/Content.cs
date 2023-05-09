using backend.Models.Enums;

namespace backend.kapace.BLL.Models.VideoService;

public record Content
{
    private Content() { }
    
    internal long Id { get; private init; }
    
    internal string Title { get;  private init; }

    internal string EngTitle { get; private init; }

    internal string OriginTitle { get; private init; }
    
    internal string Description { get;  private init; }
    
    internal ContentType Type { get;  private init; }
    
    internal ContentStatus Status { get;  private init; }
    
    internal string Image { get;  private init; }
    
    internal decimal ImportStars { get;  private init; }
    
    internal int OutSeries { get;  private init; }
    
    internal int PlannedSeries { get;  private init; }
    
    internal int Views { get;  private init; }
    
    internal int Country { get;  private init; }
    
    internal DateTimeOffset ReleasedAt { get;  private init; }
    
    internal DateTimeOffset CreatedAt { get;  private init; }
    
    internal DateTimeOffset LastUpdateAt { get;  private init; }
    
    internal int MinAgeLimit { get;  private init; }
    
    internal int? Duration { get; private init; }
    
    internal static Content ToBLLContent(DAL.Models.Content content)
    {
        return new Content
        {
            Id = content.Id,
            Title = content.Title,
            EngTitle = content.EngTitle,
            OriginTitle = content.OriginTitle,
            Description = content.Description,
            Type = (ContentType)content.Type,
            Status = (ContentStatus)content.Status,
            Image = content.Image,
            ImportStars = content.ImportStars,
            OutSeries = content.OutSeries,
            PlannedSeries = content.PlannedSeries,
            Views = content.Views,
            Country = content.Country,
            ReleasedAt = content.ReleasedAt,
            CreatedAt = content.CreatedAt,
            LastUpdateAt = content.LastUpdateAt,
            MinAgeLimit = content.MinAgeLimit,
            Duration = content.Duration
        };
    } 
};