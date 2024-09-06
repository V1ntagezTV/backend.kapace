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
    
    internal long ImageId { get; init; }
    
    internal decimal ImportStars { get; init; }
    
    internal int OutSeries { get; init; }
    
    internal int PlannedSeries { get; init; }
    
    internal int Views { get; init; }
    
    internal int Country { get; init; }
    
    internal DateTimeOffset? ReleasedAt { get; init; }
    
    internal DateTimeOffset CreatedAt { get; init; }
    
    internal DateTimeOffset LastUpdateAt { get; init; }
    
    internal int MinAgeLimit { get; init; }
    
    internal int? Duration { get; init; }
    internal string? Channel { get; init; }
}