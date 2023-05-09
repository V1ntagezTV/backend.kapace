using backend.kapace.DAL.Models;
using backend.kapace.DAL.Repository.Interfaces;
using backend.kapace.Infrastructure.Database;
using Dapper;
using Npgsql;

namespace backend.kapace.DAL.Repository;

public class EpisodeRepository : BaseKapaceRepository, IEpisodeRepository
{
    public EpisodeRepository(NpgsqlDataSource dataSource) : base(dataSource) { }

    public async Task<Episode[]> QueryAsync(QueryEpisode queryEpisode, CancellationToken token)
    {
        var initSql = "SELECT * FROM episode ";
        var parameters = new DynamicParameters();
        var filters = new List<string>();
        
        if (queryEpisode.ContentIds.Any())
        {
            filters.Add($"content_id = ANY(@{nameof(queryEpisode.ContentIds)})");
            parameters.Add(nameof(queryEpisode.ContentIds), queryEpisode.ContentIds);
        }
        
        if (filters.Any())
        {
            initSql += $" WHERE {string.Join(" AND ", filters)}";
        }
        
        await using var connection = CreateConnection();
        var command = new CommandDefinition(initSql, parameters, cancellationToken: token);
        var result = await connection.QueryAsync<Episode>(command);
        return result.ToArray();
    }
}