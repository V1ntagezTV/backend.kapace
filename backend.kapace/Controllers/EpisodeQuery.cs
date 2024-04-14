namespace backend.kapace.Controllers;

public record EpisodeQuery(
    long[]? EpisodeIds, 
    long[]? ContentIds,
    int Limit,
    int Offset);