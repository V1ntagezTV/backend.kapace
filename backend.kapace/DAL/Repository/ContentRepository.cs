using backend.kapace.DAL.Experimental;
using backend.kapace.DAL.Models;
using backend.kapace.DAL.Repository.Interfaces;
using backend.kapace.Infrastructure.Database;
using backend.kapace.Models;
using Dapper;
using Npgsql;
using QueryContent = backend.kapace.DAL.Models.QueryContent;

namespace backend.kapace.DAL.Repository;

public class ContentRepository : BaseKapaceRepository, IContentRepository
{
    private const int BaseLongTimeoutSeconds = 10;

    public ContentRepository(NpgsqlDataSource dataSource) : base(dataSource) { }

    public async Task<Content[]> QueryAsync(QueryContent query, CancellationToken token)
    {
        const string initSql = @"SELECT * FROM content WHERE 1=1";
        var command = new ExperimentalQueryBuilder(initSql)
            .CanReturnWholeTable(true)
            .WhereAny("id", query.Ids)
            .WhereAny("status", query.Statuses)
            .WhereAny("type", query.Types)
            .WhereAny("country", query.Countries)
            .AddPaging(query.Limit, query.Offset)
            .Build(token);
        
        await using var connection = CreateConnection();
        var result = await connection.QueryAsync<Content>(command);
        return result.ToArray();
    }

    public async Task<IReadOnlyCollection<Content>> GetOrderedByPopularAsync(
        int forLastDaysCount,
        QueryPaging paging,
        CancellationToken token)
    {
        const string initSql =
            """
            SELECT * FROM content
            WHERE created_at > (current_timestamp::date - @ForLastDaysCount)
            ORDER BY views DESC, import_stars DESC
            LIMIT @Limit
            OFFSET @Offset
            """;

        var parameters = new
        {
            ForLastDaysCount = forLastDaysCount,
            paging.Limit,
            paging.Offset,
        };
        
        await using var connection = CreateConnection();
        var command = new CommandDefinition(initSql, parameters, cancellationToken: token);
        var result = await connection.QueryAsync<Content>(command);
        return result.ToArray();
    }

    public async Task<IReadOnlyCollection<Content>> GetOrderedByNewestAsync(
        QueryPaging paging,
        CancellationToken token)
    {
        const string initSql = 
            """
            SELECT * FROM content
            ORDER BY created_at DESC
            LIMIT @Limit
            OFFSET @Offset
            """;

        var parameters = new
        {
            paging.Limit,
            paging.Offset,
        };
        
        await using var connection = CreateConnection();
        var command = new CommandDefinition(initSql, parameters, cancellationToken: token);
        var result = await connection.QueryAsync<Content>(command);
        return result.ToArray();
    }
    
    public async Task<IReadOnlyCollection<Content>> GetOrderedByLastUpdatedAsync(
        QueryPaging paging,
        CancellationToken token)
    {
        const string initSql = @"
SELECT * FROM content
ORDER BY last_update_at DESC
LIMIT @Limit
OFFSET @Offset";

        var parameters = new
        {
            paging.Limit,
            paging.Offset
        };
        
        await using var connection = CreateConnection();
        var command = new CommandDefinition(initSql, parameters, cancellationToken: token);
        var result = await connection.QueryAsync<Content>(command);
        return result.ToArray();
    }

    public async Task<long> InsertAsync(InsertContentQuery query, CancellationToken token)
    {
        const string sql = $@"
            insert into content(id,
                                title,
                                description,
                                type,
                                status,
                                image_id,
                                import_stars,
                                out_series,
                                planned_series,
                                views,
                                country,
                                released_at,
                                created_at,
                                last_update_at,
                                min_age_limit,
                                duration,
                                eng_title,
                                channel,
                                origin_title)
                        values (@Id,
                                @Title,
                                @Description,
                                @ContentType,
                                @Status, 
                                @ImageId, 
                                @ImportStars,
                                @OutSeries,
                                @PlannedSeries,
                                @Views, 
                                @Country,
                                @ReleasedAt,
                                @CreatedAt,
                                @LastUpdateAt,
                                @MinAge,
                                @Duration, 
                                @EngTitle, 
                                @Channel,
                                @OriginTitle) 
                        returning id;";

        var parameters = new
        {
            query.Id,
            query.Title,
            query.Description,
            query.ContentType,
            query.Status,
            query.ImageId,
            query.ImportStars,
            query.OutSeries,
            query.PlannedSeries,
            query.Views,
            query.Country,
            query.ReleasedAt,
            query.CreatedAt,
            query.LastUpdateAt,
            query.MinAge,
            query.Duration,
            query.EngTitle,
            query.Channel,
            query.OriginTitle,
        };
        
        await using var connection = CreateConnection();
        var command = new CommandDefinition(sql, parameters, cancellationToken: token);
        return await connection.QueryFirstOrDefaultAsync<long>(command);
    }

    public async Task IncrementViews(long contentId, CancellationToken token)
    {
        const string initSql = "UPDATE content SET views = views + 1 WHERE 1=1";

        var command = new ExperimentalQueryBuilder(initSql)
            .Where("id", contentId)
            .Build(token);
        
        await using var connection = CreateConnection();
        await connection.QueryAsync(command);
    }
    
    public async Task IncrementOutEpisodesCounter(long contentId, CancellationToken token)
    {
        const string initSql = "UPDATE content SET out_series = out_series + 1 WHERE 1=1";

        var command = new ExperimentalQueryBuilder(initSql)
            .Where("id", contentId)
            .Build(token);
        
        await using var connection = CreateConnection();
        await connection.QueryAsync(command);
    }

    public async Task UpdateAsync(ContentUpdateQuery model, CancellationToken token)
    {
        const string initSql = $@"
            UPDATE content SET
                image_id = @{nameof(model.ImageId)},
                title = @{nameof(model.Title)},
                eng_title = @{nameof(model.EngTitle)},
                origin_title = @{nameof(model.OriginTitle)},
                description = @{nameof(model.Description)},
                country = @{nameof(model.Country)},
                type = @{nameof(model.ContentType)},
                duration = @{nameof(model.Duration)},
                released_at = @{nameof(model.ReleasedAt)},
                planned_series = @{nameof(model.PlannedSeries)},
                min_age_limit = @{nameof(model.MinAge)}
            WHERE id = @{nameof(model.ContentId)};";
        
        var parameters = new
        {
            model.ContentId,
            model.ImageId,
            model.Title,
            model.EngTitle,
            model.OriginTitle,
            model.Description,
            model.Country,
            model.ContentType,
            model.Duration,
            model.ReleasedAt,
            model.PlannedSeries,
            model.MinAge,
        };
        
        await using var connection = CreateConnection();
        var command = new CommandDefinition(initSql, parameters, cancellationToken: token);
        await connection.QueryAsync(command);
    }

    public async Task<IReadOnlyCollection<Content>> SearchByText(string? search, CancellationToken token)
    {
        const string initSql = @$"SELECT * FROM content WHERE title ILIKE CONCAT('%',@Search,'%');";

        var parameters = new
        {
            Search = search
        };
        
        await using var connection = CreateConnection();
        var command = new CommandDefinition(
            initSql,
            parameters,
            commandTimeout: BaseLongTimeoutSeconds,
            cancellationToken: token);
        var result = await connection.QueryAsync<Content>(command);
        return result.ToArray();
    }
}