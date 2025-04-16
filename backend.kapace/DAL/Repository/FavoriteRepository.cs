using backend.kapace.DAL.Experimental;
using backend.kapace.DAL.Models;
using backend.kapace.DAL.Models.Query;
using backend.kapace.DAL.Repository.Interfaces;
using backend.kapace.Infrastructure.Database;
using backend.Models.Enums;
using Dapper;
using Npgsql;

namespace backend.kapace.DAL.Repository;

public class FavoriteRepository(NpgsqlDataSource npgsqlDataSource)
    : BaseKapaceRepository(npgsqlDataSource), IFavoriteRepository
{
    public async Task<IReadOnlyCollection<Favorite>> Query(FavoriteQuery query, CancellationToken token)
    {
        const string initSql = "SELECT * FROM favorites WHERE 1 = 1";
        var command = new ExperimentalQueryBuilder(initSql)
            .WhereAny("user_id", query.UserIds)
            .WhereAny("content_id", query.ContentIds)
            .WhereAny("episode_id", query.EpisodeIds)
            .WhereAny("status", query.Statuses?.Select(x => (int)x).ToArray())
            .OrderBy(query.IsOrderByCreated ? "created_at" : null)
            .AddPaging(query.Limit, query.Offset)
            .Build(token);
        
        await using var connection = CreateConnection();
        var result = await connection.QueryAsync<Favorite>(command);
        return result.ToArray();
    }

    public async Task<IReadOnlyCollection<Favorite>> Upsert(Favorite[] models, CancellationToken token)
    {
        const string initSql = """
                               INSERT INTO favorites(
                                  user_id,
                                  content_id,
                                  episode_id,
                                  status,
                                  stars,
                                  created_at)
                               VALUES(
                                   unnest(@UserIds),
                                   unnest(@ContentIds),
                                   unnest(@EpisodeIds),
                                   unnest(@Statuses),
                                   unnest(@Stars),
                                   unnest(@CreatedAt))
                               ON CONFLICT(user_id, content_id) DO UPDATE SET
                                  stars = excluded.stars,
                                  episode_id = excluded.episode_id,
                                  status = excluded.status;
                               """;

        var parameters = new
        {
            UserIds = models.Select(x => x.UserId).ToArray(),
            ContentIds = models.Select(x => x.ContentId).ToArray(),
            EpisodeIds = models.Select(x => x.EpisodeId).ToArray(),
            Statuses = models.Select(x => x.Status).ToArray(),
            Stars = models.Select(x => x.Stars).ToArray(),
            CreatedAt = models.Select(x => x.CreatedAt).ToArray(),
        };
        
        await using var connection = CreateConnection();
        var command = new CommandDefinition(initSql, parameters, cancellationToken: token);
        var response = await connection.QueryAsync<Favorite>(command);

        return response.ToArray();
    }

    public async Task SetEpisode(long userId, long contentId, long episodeId, CancellationToken token)
    {
        const string initSql = """
                               UPDATE favorites SET episode_id = @EpisodeId 
                               WHERE user_id = @UserId AND content_id = @ContentId
                               """;

        var parameters = new
        {
            EpisodeId = episodeId,
            ContentId = contentId,
            UserId = userId
        };

        var command = new CommandDefinition(
            initSql,
            parameters,
            commandTimeout: DefaultTimeoutInSeconds,
            cancellationToken: token);
        
        await using var connection = CreateConnection();
        await connection.ExecuteAsync(command);
    }

    public async Task Remove(long userId, long[] ids, CancellationToken token)
    {
        const string initSql = "DELETE FROM favorites WHERE 1 = 1";
        var command = new ExperimentalQueryBuilder(initSql)
            .Where("user_id", userId)
            .WhereAny("id", ids)
            .Build(token);
        
        await using var connection = CreateConnection();
        await connection.ExecuteAsync(command);
    }

    public async Task SetStatus(long userId, long contentId, FavouriteStatus? status, CancellationToken token)
    {
        const string initSql = """
                               UPDATE favorites SET status = @Status 
                               WHERE user_id = @UserId AND content_id = @ContentId
                               """;

        var parameters = new
        {
            Status = status,
            ContentId = contentId,
            UserId = userId
        };

        var command = new CommandDefinition(
            initSql,
            parameters,
            commandTimeout: DefaultTimeoutInSeconds,
            cancellationToken: token);
        
        await using var connection = CreateConnection();
        await connection.ExecuteAsync(command);
    }

    public async Task SetStars(long userId, long contentId, int stars, CancellationToken token)
    {
        const string initSql = """
                               UPDATE favorites SET stars = @Stars 
                               WHERE user_id = @UserId AND content_id = @ContentId
                               """;

        var parameters = new
        {
            Stars = stars,
            ContentId = contentId,
            UserId = userId
        };

        var command = new CommandDefinition(
            initSql,
            parameters,
            commandTimeout: DefaultTimeoutInSeconds,
            cancellationToken: token);
        
        await using var connection = CreateConnection();
        await connection.ExecuteAsync(command);
    }
}