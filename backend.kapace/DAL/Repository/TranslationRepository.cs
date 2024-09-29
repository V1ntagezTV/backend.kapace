using backend.kapace.DAL.Experimental;
using backend.kapace.DAL.Models;
using backend.kapace.DAL.Models.ContentTranslation;
using backend.kapace.DAL.Models.ContentTranslation.Query;
using backend.kapace.DAL.Repository.Interfaces;
using backend.kapace.Infrastructure.Database;
using Dapper;
using Npgsql;

namespace backend.kapace.DAL.Repository;

public class TranslationRepository : BaseKapaceRepository, ITranslationRepository
{
    public TranslationRepository(NpgsqlDataSource npgsqlDataSource) : base(npgsqlDataSource) { }
    
    
    public async Task<IReadOnlyCollection<BaseTranslation>> QueryAsync(
        long[]? contentIds,
        long[]? episodeIds,
        long[]? translationIds,
        CancellationToken token)
    {
        const string initSql = """
            SELECT 
                ct.id as translation_id,
                ct.*, 
                t.id as translator_id,
                t.name as translator_name,
                t.link as translator_link 
            FROM content_translation ct 
            LEFT JOIN translator t on t.id = ct.translator_id 
            WHERE 1 = 1 
        """;

        var command = new ExperimentalQueryBuilder(initSql)
            .WhereAny("ct.content_id", contentIds)
            .WhereAny("ct.episode_id", episodeIds)
            .WhereAny("ct.translator_id", translationIds)
            .Build(token);

        await using var connection = CreateConnection();
        var result = await connection.QueryAsync<BaseTranslation>(command);
        
        return result.ToArray();
    }

    public async Task<IReadOnlyCollection<EpisodeTranslation>> Select(Select model, CancellationToken token)
    {
        var command = new ExperimentalQueryBuilder("SELECT * FROM content_translation WHERE 1=1")
            .WhereAny("episode_id", model.EpisodeIds)
            .WhereAny("id", model.EpisodeTranslationIds)
            .WhereAny("content_id", model.ContentIds)
            .WhereAny("translator_id", model.TranslatorIds)
            .AddPaging(model.Limit, model.Offset)
            .Build(token);

        await using var connection = CreateConnection();
        var result = await connection.QueryAsync<EpisodeTranslation>(command);
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
                language, 
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