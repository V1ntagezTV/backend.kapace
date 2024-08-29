using backend.kapace.DAL.Experimental;
using backend.kapace.DAL.Models;
using backend.kapace.DAL.Repository.Interfaces;
using backend.kapace.Infrastructure.Database;
using Dapper;
using Npgsql;

namespace backend.kapace.DAL.Repository;

public class EpisodeRepository : BaseKapaceRepository, IEpisodeRepository
{
    public EpisodeRepository(NpgsqlDataSource dataSource) : base(dataSource) { }

    public async Task<Episode[]> QueryAsync(QueryEpisode query, CancellationToken token)
    {
        var command = new ExperimentalQueryBuilder("SELECT * FROM episode WHERE 1=1")
            .WhereAny("id", query.EpisodeIds)
            .WhereAny("content_id", query.ContentIds)
            .WhereAny("number", query.Numbers)
            .AddPaging(query.Limit, query.Offset)
            .Build(token);
        
        await using var connection = CreateConnection();
        var result = await connection.QueryAsync<Episode>(command);
        return result.ToArray();
    }

    public async Task<long> InsertAsync(Episode model, CancellationToken token)
    {
        const string initSql =
            @$"INSERT INTO episode(content_id, title, image, number) 
               VALUES (@ContentId, @Title, @Image, @Number)
               RETURNING id;";

        var parameters = new
        {
            model.ContentId,
            model.Title,
            model.Image,
            model.Number
        };

        await using var connection = CreateConnection();
        var command = new CommandDefinition(initSql, parameters, cancellationToken: token);
        return await connection.QuerySingleAsync<long>(command);
    }

    public async Task UpdateAsync(Episode episode, CancellationToken token)
    {
        const string initSql = $@"
            UPDATE episode SET
                content_id = @{nameof(episode.ContentId)}
                image = @{nameof(episode.Image)},
                title = @{nameof(episode.Title)},
                number = @{nameof(episode.Number)}
            WHERE id = @{nameof(episode.Id)};";

        var parameters = new
        {
            episode.ContentId,
            episode.Id,
            episode.Image,
            episode.Title,
            episode.Number
        };
        
        await using var connection = CreateConnection();
        var command = new CommandDefinition(initSql, parameters, cancellationToken: token);
        await connection.QueryAsync(command);
    }
}