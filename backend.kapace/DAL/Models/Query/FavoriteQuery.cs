using backend.Models.Enums;

namespace backend.kapace.DAL.Models.Query;

public class FavoriteQuery
{
    public long[]? UserIds { get; set; }
    public long[]? ContentIds { get; set; }
    public long[]? EpisodeIds { get; set; }
    public FavouriteStatus[]? Statuses { get; set; }
    public int? Limit { get; set; }
    public int? Offset { get; set; }
    public bool IsOrderByCreated { get; set; }
}