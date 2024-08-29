using backend.kapace.DAL.Models;
using backend.kapace.DAL.Repository.Interfaces;
using backend.kapace.Infrastructure.Database;
using backend.kapace.Models;
using Dapper;
using Npgsql;

namespace backend.kapace.DAL.Repository;

public class ContentRepository : BaseKapaceRepository, IContentRepository
{
    private const int BaseLongTimeoutSeconds = 10;

    public ContentRepository(NpgsqlDataSource dataSource) : base(dataSource) { }

    public async Task<Content[]> QueryAsync(QueryContent query, CancellationToken token)
    {
        var initSql = @"SELECT * FROM content";
        var parameters = new DynamicParameters();
        var filters = new List<string>();
        
        if (query.Ids is {Length: > 0})
        {
            parameters.Add(nameof(query.Ids), query.Ids);
            filters.Add($"id = ANY(@{nameof(query.Ids)})");
        }

        if (query.Statuses is { Length: > 0 })
        {
            parameters.Add(nameof(query.Statuses), query.Statuses);
            filters.Add($"status = ANY(@{nameof(query.Statuses)})");
        }
        
        if (query.Types is { Length: > 0 })
        {
            parameters.Add(nameof(query.Types), query.Types);
            filters.Add($"type = ANY(@{nameof(query.Types)})");
        }

        if (query.Countries is { Length: > 0 })
        {
            parameters.Add(nameof(query.Countries), query.Countries);
            filters.Add($"country = ANY(@{nameof(query.Countries)})");
        }

        if (filters.Any())
        {
            initSql += $" WHERE {string.Join(" AND ", filters)} ";
        }

        if (query.Limit > 0)
        {
            parameters.Add(nameof(query.Limit), query.Limit);
            initSql += $" LIMIT @{nameof(query.Limit)} ";
        }

        if (query.Offset > 0)
        {
            parameters.Add(nameof(query.Offset), query.Offset);
            initSql += $" OFFSET @{nameof(query.Offset)} ";
        }

        Console.WriteLine(initSql);
        await using var connection = CreateConnection();
        var command = new CommandDefinition(initSql, parameters, cancellationToken: token);
        var result = await connection.QueryAsync<Content>(command);
        return result.ToArray();
    }

    public async Task<IReadOnlyCollection<Content>> GetOrderedByPopularAsync(
        int forLastDaysCount,
        QueryPaging paging,
        CancellationToken token)
    {
        const string initSql = @"
SELECT * FROM content
WHERE created_at > (current_timestamp::date - @ForLastDaysCount)
ORDER BY views DESC, import_stars DESC
LIMIT @Limit
OFFSET @Offset";

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
        const string initSql = @"
SELECT * FROM content
ORDER BY created_at DESC
LIMIT @Limit
OFFSET @Offset";

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
            OriginalTitle = model.OriginTitle,
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
        const string initSql = @$"SELECT * FROM content WHERE title LIKE CONCAT('%',@Search,'%');";

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