using backend.kapace.BLL.Enums;
using backend.Models.Enums;

namespace backend.kapace.BLL.Models;

public sealed record InsertContentModel(
    long Id,
    long ImageId, 
    string Title, 
    string Description,
    ContentType ContentType, 
    Country Country,
    
    string? OriginTitle, 
    string? EngTitle, 
    ContentStatus? Status,
    string? Channel,
    int? MinAge,
    int? Duration,
    int? PlannedSeries, 
    DateTimeOffset? ReleasedAt
);