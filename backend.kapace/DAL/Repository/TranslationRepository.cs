using backend.kapace.DAL.Models;
using backend.kapace.DAL.Repository.Interfaces;
using backend.kapace.Infrastructure.Database;
using Dapper;
using Npgsql;

namespace backend.kapace.DAL.Repository;

public class TranslationRepository : BaseKapaceRepository, ITranslationRepository
{
    public TranslationRepository(NpgsqlDataSource npgsqlDataSource) : base(npgsqlDataSource) { }

    public async Task<IReadOnlyCollection<Translation>> QueryAsync(
        long[]? episodeIds, 
        long[]? translationIds,
        CancellationToken token)
    {
        var initSql = @"
            SELECT * FROM content_translation ct
            LEFT JOIN translator t on ct.translator_id = t.id
            LEFT JOIN episode e on ct.episode_id = e.id
            WHERE 1 = 1";

        var parameters = new DynamicParameters();
        var whereFilters = new List<string>();
        
        if (episodeIds?.Any() is true)
        {
            parameters.Add($"@EpisodeIds", episodeIds);
            whereFilters.Add("episode_id = ANY(@EpisodeIds)");
        }

        if (translationIds?.Any() is true) 
        {
            parameters.Add($"@Translator_ids", translationIds);
            whereFilters.Add("translator_id = ANY(@Translator_ids)");
        }

        initSql += $" AND {string.Join(" AND ", whereFilters)}";
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

    public async Task<long> InsertAsync(InsertTranslation translation, CancellationToken token)
    {
        const string initSql = @"
            INSERT INTO content_translation(
                content_id,
                translator_id, 
                episode_id,
                lang, 
                link,
                translation_type,
                created_at,
                created_by,
                quality)
            VALUES (
                @ContentId, 
                @TranslatorId, 
                @EpisodeId, 
                @Lang, 
                @Link, 
                @TranslationType, 
                @CreatedAt, 
                @CreatedBy, 
                @Quality)
            RETURNING id;";

        var parameters = new 
        {
            translation.ContentId,
            translation.TranslatorId,
            translation.EpisodeId,
            translation.Lang,
            translation.Link,
            translation.TranslationType,
            translation.CreatedAt,
            translation.CreatedBy,
            translation.Quality,
        };

        await using var connection = CreateConnection();
        var command = new CommandDefinition(initSql, parameters, cancellationToken: token);
        var id = await connection.QueryFirstAsync<long>(command);

        return id;
    }
}