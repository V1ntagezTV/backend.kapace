namespace backend.kapace.BLL.Models;

public record UpdateContentModel(
    long ContentId,
    long? ImageId, 
    string? Title, 
    string? EngTitle, 
    string? OriginalTitle, 
    string? Description, 
    int? Country,
    int? ContentType, 
    int? Duration, 
    DateTimeOffset? ReleasedAt,
    int? PlannedSeries,
    int? MinAge);