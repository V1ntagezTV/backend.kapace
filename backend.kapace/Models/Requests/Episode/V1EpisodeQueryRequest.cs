namespace backend.kapace.Controllers;

public record V1EpisodeQueryRequest(
    long[]? EpisodeIds,
    long[]? ContentIds,
    int Limit, 
    int Offset);