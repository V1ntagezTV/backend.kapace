namespace backend.kapace.BLL.Models.Episode;

public class EpisodeQuery
{
    public long[]? EpisodeIds { get; init; }
    public long[]? ContentIds { get; init; }
    public int Limit { get; init; }
    public int Offset { get; init; }
};