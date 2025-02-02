using System.Data;
using backend.kapace.DAL.Models;
using backend.kapace.DAL.Repository.Interfaces;
using backend.kapace.Infrastructure.Database;
using Dapper;
using Npgsql;

namespace backend.kapace.DAL.Repository;

public class ContentGenreRepository : BaseKapaceRepository, IContentGenreRepository
{
    private readonly NpgsqlDataSource _npgsqlDataSource;

    public ContentGenreRepository(NpgsqlDataSource npgsqlDataSource) : base(npgsqlDataSource)
    {
        _npgsqlDataSource = npgsqlDataSource;
    }

    public async Task Insert(ContentGenreV1[] contentGenres, CancellationToken token)
    {
        const string sql =
            @"
            INSERT INTO content_genre(content_id, genre_id, created_at, created_by) 
            SELECT * FROM unnest(@Values);
            ";

        var parameters = new
        {
            Values = contentGenres
        };

        await using var connection = _npgsqlDataSource.CreateConnection();
        var command = new CommandDefinition(sql, parameters, cancellationToken: token);
        await connection.QueryAsync(command);
    }
    
    public async Task<TQueryResult[]> QueryAsync<TQueryResult>(QueryContentGenre query, CancellationToken token)
        where TQueryResult: ContentGenreV1
    {
        var initSql = @"SELECT * FROM content_genre";
        var parameters = new DynamicParameters();
        var filters = new List<string>();
        
        if (query.ContentIds is {Length: > 0})
        {
            parameters.Add(nameof(query.ContentIds), query.ContentIds);
            filters.Add($"content_id = ANY(@{nameof(query.ContentIds)})");
        }

        if (query.GenreIds.Any()) 
        {
            parameters.Add(nameof(query.GenreIds), query.GenreIds);
            filters.Add($"genre_id = @{nameof(query.GenreIds)}");
        }

        if (typeof(TQueryResult) == typeof(ContentGenreV1.WithName))
        {
            initSql += " JOIN genre ON genre.id = content_genre.genre_id ";
        }
        
        if (filters.Count != 0)
        {
            initSql += $" WHERE {string.Join(" AND ", filters)}";
        }

        await using var connection = CreateConnection();
        var result = await connection.QueryAsync<TQueryResult>(initSql, parameters);
        return result.ToArray();
    }

    public async Task<ContentGenreV1.WithName[]> GetByContentIdsAsync(long[] contentIds, CancellationToken token)
    {
        const string initSql = @"
SELECT * FROM content_genre cg
JOIN genre g on g.id = cg.genre_id
WHERE content_id = ANY(@ContentIds);";

        var parameters = new
        {
            ContentIds = contentIds,
        };

        await using var connection = CreateConnection();
        var result = await connection.QueryAsync<ContentGenreV1.WithName>(
            initSql,
            parameters);
        
        return result.ToArray();
    }
}