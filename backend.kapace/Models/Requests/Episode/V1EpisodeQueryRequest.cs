namespace backend.kapace.Models.Requests.Episode;

public record V1EpisodeQueryRequest(
    long[]? EpisodeIds,
    long[]? ContentIds,
    int Limit, 
    int Offset);