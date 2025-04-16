using backend.kapace.DAL.Models;
using backend.Models.Enums;
using Content = backend.kapace.BLL.Models.VideoService.Content;

namespace backend.kapace.BLL.Services.Interfaces;

public interface IFavoriteService
{
    Task SetEpisode(long userId, long contentId, long episodeId, CancellationToken token);
    Task SetStatus(long userId, long contentId, FavouriteStatus? status, CancellationToken token);
    Task SetStars(long userId, long contentId, int stars, CancellationToken token);
    Task<Dictionary<long, (Favorite, Content)>> GetMap(
        long userId,
        FavouriteStatus? status,
        int limit,
        int offset,
        CancellationToken token);
}