using backend.Models.Enums;

namespace backend.kapace.DAL.Models;

public class QueryEpisode
{
    public long[]? EpisodeIds {get; init; }
    public long[]? ContentIds { get; init; }
    public long[]? Numbers { get; init; }
    public int Limit { get; init; }
    public int Offset { get; init; }
    public EpisodeOrderType? OrderBy { get; init; }
}