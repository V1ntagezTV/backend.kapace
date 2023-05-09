using backend.kapace.DAL.Models;
using backend.kapace.DAL.Repository.Interfaces;
using backend.kapace.Infrastructure.Database;
using backend.kapace.Models;
using Dapper;
using Microsoft.AspNetCore.Identity;
using Npgsql;

namespace backend.kapace.DAL.Repository;

public class ContentRepository : BaseKapaceRepository, IContentRepository
{
    public ContentRepository(NpgsqlDataSource dataSource) : base(dataSource) { }

    public async Task<Content[]> QueryAsync(QueryContent query, CancellationToken token)
    {
        var initSql = @"SELECT * FROM content";
        var parameters = new DynamicParameters();
        var filters = new List<string>();
        
        if (query.Ids.Any())
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

        await using var connection = CreateConnection();
        Console.WriteLine(initSql);
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
}