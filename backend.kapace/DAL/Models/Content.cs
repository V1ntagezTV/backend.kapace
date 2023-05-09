namespace backend.kapace.DAL.Models;

public class Content
{

    internal long Id { get; init; }
    
    internal string Title { get; init; }

    internal string EngTitle { get; init; }

    internal string OriginTitle { get; init; }
    
    internal string Description { get; init; }
    
    internal int Type { get; init; }
    
    internal int Status { get; init; }
    
    internal string Image { get; init; }
    
    internal decimal ImportStars { get; init; }
    
    internal int OutSeries { get; set; }
    
    internal int PlannedSeries { get; set; }
    
    internal int Views { get; set; }
    
    internal int Country { get; set; }
    
    internal DateTimeOffset ReleasedAt { get; set; }
    
    internal DateTimeOffset CreatedAt { get; set; }
    
    internal DateTimeOffset LastUpdateAt { get; set; }
    
    internal int MinAgeLimit { get; set; }
    
    internal int? Duration { get; set; }
}