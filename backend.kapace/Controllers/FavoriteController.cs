using backend.kapace.BLL.Services.Interfaces;
using backend.kapace.DAL.Models;
using backend.kapace.DAL.Repository.Interfaces;
using backend.kapace.Middlewares;
using backend.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.kapace.Controllers;

[ApiController]
[Route("v1/favorite")]
public class FavoriteController(
    IFavoriteRepository repository,
    IFavoriteService service) : ApiController
{
    [Authorize]
    [HttpPost("query")]
    public async Task<IActionResult> Query(FavoriteQuery.Request request, CancellationToken token)
    {
        var favorites = await repository.Query(new()
        {
            UserIds = [User.GetUserId()],
            ContentIds = request.ContentIds,
            EpisodeIds = request.EpisodeIds,
            Limit = request.Limit,
            Offset = request.Offset
        }, token);

        var data = favorites
            .Select(f =>
                new FavoriteQuery.Response.Favorite(
                    f.Id,
                    f.UserId,
                    f.ContentId,
                    (FavouriteStatus)f.Status,
                    f.EpisodeId,
                    f.Stars,
                    f.CreatedAt))
            .ToArray();

        return Ok(new FavoriteQuery.Response(data));
    }
    
    [Authorize]
    [HttpPost("get-list")]
    public async Task<IActionResult> GetList(GetFavoritesListRequest request, CancellationToken token)
    {
        var favoritesMap = await service.GetMap(
            User.GetUserId(),
            request.Status,
            request.Limit,
            request.Offset,
            token);

        var favorites = favoritesMap
            .Select(pair =>
            {
                var content = pair.Value.Item2;
                var favorite = pair.Value.Item1;
                
                return new
                {
                    Id = pair.Key,
                    UserId = favorite.UserId,
                    ContentId = content.Id,
                    Title = content.Title,
                    Status = (FavouriteStatus?)favorite.Status,
                    EpisodeId = favorite.EpisodeId,
                    Stars = favorite.Stars,
                    CreatedAt = favorite.CreatedAt
                };
            }).ToArray();

        return Ok(new { Favorites = favorites });
    }
    
    [Authorize]
    [HttpPost("insert")]
    public async Task<IActionResult> Insert(FavoriteInsert.Request request, CancellationToken token)
    {
        var data = request.Contents.Select(c => new Favorite
        {
            UserId = User.GetUserId(),
            ContentId = c.ContentId,
            Status = (int)request.Status,
            EpisodeId = c.EpisodeId,
            Stars = c.Stars,
            CreatedAt = DateTimeOffset.UtcNow
        }).ToArray();

        await repository.Upsert(data, token);

        return Ok();
    }

    [Authorize]
    [HttpPost("set-episode")]
    public async Task<IActionResult> SetEpisode(FavoriteSetEpisodeRequest request, CancellationToken token)
    {
        await service.SetEpisode(User.GetUserId(), request.ContentId, request.EpisodeId, token);

        return Ok();
    }
    
    [Authorize]
    [HttpPost("set-status")]
    public async Task<IActionResult> SetStatus(FavoriteSetStatusRequest request, CancellationToken token)
    {
        await service.SetStatus(User.GetUserId(), request.ContentId, request.Status, token);
        
        return Ok();
    }
    
    [Authorize]
    [HttpPost("set-stars")]
    public async Task<IActionResult> SetStars(FavoriteSetStarsRequest request, CancellationToken token)
    {
        await service.SetStars(User.GetUserId(), request.ContentId, request.Stars, token);
        
        return Ok();
    }
}

public record GetFavoritesListRequest(
    FavouriteStatus? Status,
    int Limit,
    int Offset);
public sealed record FavoriteSetEpisodeRequest(long ContentId, long EpisodeId);
public sealed record FavoriteSetStatusRequest(long ContentId, FavouriteStatus? Status);

public sealed record FavoriteSetStarsRequest(long ContentId, int Stars);

public sealed class FavoriteInsert
{
    public record Request(
        Request.Content[] Contents, 
        FavouriteStatus Status)
    {
        public record Content(
            long ContentId,
            long? EpisodeId,
            int Stars);
    }
}

public sealed class FavoriteQuery
{
    public record Request(
        long[] ContentIds,
        long[] EpisodeIds,
        int Limit,
        int Offset);
    
    public record Response(Response.Favorite[] Favorites)
    {
        public record Favorite(
            long Id,
            long UserId,
            long ContentId,
            FavouriteStatus Status,
            long? EpisodeId,
            int? Stars,
            DateTimeOffset CreatedAt);
    }
}