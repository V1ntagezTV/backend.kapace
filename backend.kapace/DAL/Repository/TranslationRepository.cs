using backend.kapace.DAL.Models;
using backend.kapace.DAL.Repository.Interfaces;
using backend.kapace.Infrastructure.Database;
using Dapper;
using Npgsql;

namespace backend.kapace.DAL.Repository;

public class TranslationRepository : BaseKapaceRepository, ITranslationRepository
{
    public TranslationRepository(NpgsqlDataSource npgsqlDataSource) : base(npgsqlDataSource) { }

    public async Task<IReadOnlyCollection<Translation>> GetByEpisodeAsync(long episodeId, CancellationToken token)
    {
        const string initSql = @$"
            SELECT * FROM content_translation 
            WHERE episode_id = @EpisodeId;";

        var parameters = new
        {
            EpisodeId = episodeId,
        };

        await using var connection = CreateConnection();
        var command = new CommandDefinition(initSql, parameters, cancellationToken: token);
        var result = await connection.QueryAsync<Translation>(command);
        return result.ToArray();
    }

    public async Task<IReadOnlyCollection<Translation>> GetByContentsAsync(long[] contentIds, CancellationToken token)
    {
        const string initSql = @"
            SELECT * FROM content_translation 
            WHERE content_id = ANY(@ContentIds);";

        var parameters = new
        {
            ContentIds = contentIds,
        };

        await using var connection = CreateConnection();
        var command = new CommandDefinition(initSql, parameters, cancellationToken: token);
        var result = await connection.QueryAsync<Translation>(command);
        return result.ToArray();
    }
}