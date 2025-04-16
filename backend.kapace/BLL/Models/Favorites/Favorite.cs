using backend.Models.Enums;

namespace backend.kapace.BLL.Models.Favorites;

public record Favorite(
    long Id,
    long UserId,
    long ContentId,
    FavouriteStatus Status,
    long? EpisodeId,
    decimal? Stars,
    DateTimeOffset CreatedAt);