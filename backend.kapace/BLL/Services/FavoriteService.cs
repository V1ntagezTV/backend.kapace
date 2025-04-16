using backend.kapace.BLL.Enums;
using backend.kapace.BLL.Models;
using backend.kapace.BLL.Services.Interfaces;
using backend.kapace.DAL.Models;
using backend.kapace.DAL.Models.Query;
using backend.kapace.DAL.Repository.Interfaces;
using backend.Models.Enums;
using Content = backend.kapace.BLL.Models.VideoService.Content;

namespace backend.kapace.BLL.Services;

public class FavoriteService(
    IFavoriteRepository favoritesRepository,
    IContentService contentService
    ) : IFavoriteService
{
    public async Task SetEpisode(long userId, long contentId, long episodeId, CancellationToken token)
    {
        var favoritesQuery = await favoritesRepository.Query(new FavoriteQuery()
        {
            UserIds = [userId],
            ContentIds = [contentId]
        }, token);

        if (favoritesQuery.Count > 0)
        {
            await favoritesRepository.SetEpisode(userId, contentId, episodeId, token);
        }
        else
        {
            await favoritesRepository.Upsert([
                new Favorite
                {
                    UserId = userId,
                    ContentId = contentId,
                    Status = (int)FavouriteStatus.Stash,
                    EpisodeId = episodeId,
                    Stars = 0,
                    CreatedAt = DateTimeOffset.UtcNow
                }
            ], token);
        }
    }

    public async Task SetStatus(long userId, long contentId, FavouriteStatus? status, CancellationToken token)
    {
        var favoritesQuery = await favoritesRepository.Query(new FavoriteQuery()
        {
            UserIds = [userId],
            ContentIds = [contentId]
        }, token);

        if (favoritesQuery.Count > 0)
        {
            await favoritesRepository.SetStatus(userId, contentId, status, token);
        }
        else
        {
            await favoritesRepository.Upsert([
                new Favorite
                {
                    UserId = userId,
                    ContentId = contentId,
                    Status = (int?)status,
                    EpisodeId = null,
                    Stars = 0,
                    CreatedAt = DateTimeOffset.UtcNow
                }
            ], token);
        }
    }

    public async Task SetStars(long userId, long contentId, int stars, CancellationToken token)
    {
        if (stars is > 10 or < 0)
        {
            throw new ArgumentException("Stars must be between 0 and 10.", nameof(stars));
        }
        
        var favoritesQuery = await favoritesRepository.Query(new FavoriteQuery()
        {
            UserIds = [userId],
            ContentIds = [contentId]
        }, token);

        if (favoritesQuery.Count > 0)
        {
            await favoritesRepository.SetStars(userId, contentId, stars, token);
        }
        else
        {
            var newFavorite = new Favorite
            {
                UserId = userId,
                ContentId = contentId,
                Status = (int)FavouriteStatus.Stash,
                EpisodeId = null,
                Stars = stars,
                CreatedAt = DateTimeOffset.UtcNow
            };
            
            await favoritesRepository.Upsert([newFavorite], token);
        }
    }

    public async Task<Dictionary<long, (Favorite, Content)>> GetMap(long userId,
        FavouriteStatus? status,
        int limit,
        int offset,
        CancellationToken token)
    {
        var favorites = await favoritesRepository.Query(new()
        {
            Statuses = status is null ? null : [status.Value],
            UserIds = [userId],
            Limit = limit,
            Offset = offset,
            IsOrderByCreated = true,
        }, token);

        var contentIds = favorites.Select(x => x.ContentId).ToArray();

        var contents = await contentService.QueryAsync(new ContentQuery { Ids = contentIds }, token);
        var contentsMap = contents.ToDictionary(x => x.Id);

        return favorites.ToDictionary(
            favorite => favorite.ContentId,
            favorite => (favorite, contentsMap[favorite.ContentId]));
    }
}