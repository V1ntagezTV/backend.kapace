using backend.kapace.DAL.Models;
using backend.kapace.DAL.Models.Query;
using backend.Models.Enums;

namespace backend.kapace.DAL.Repository.Interfaces;

public interface IFavoriteRepository
{
    Task<IReadOnlyCollection<Favorite>> Query(FavoriteQuery query, CancellationToken token);
    Task<IReadOnlyCollection<Favorite>> Upsert(Favorite[] models, CancellationToken token);
    Task Remove(long userId, long[] ids, CancellationToken token);
    Task SetEpisode(long userId, long contentId, long episodeId, CancellationToken token);
    Task SetStatus(long userId, long contentId, FavouriteStatus? status, CancellationToken token);
    Task SetStars(long userId, long contentId, int stars, CancellationToken token);
}