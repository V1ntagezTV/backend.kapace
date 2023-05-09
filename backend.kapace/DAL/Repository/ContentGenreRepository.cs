using System.Data;
using backend.kapace.DAL.Models;
using backend.kapace.DAL.Repository.Interfaces;
using backend.kapace.Infrastructure.Database;
using Dapper;
using Npgsql;

namespace backend.kapace.DAL.Repository;

public class ContentGenreRepository : BaseKapaceRepository, IContentGenreRepository
{
    public ContentGenreRepository(NpgsqlDataSource npgsqlDataSource) : base(npgsqlDataSource) { }

    public async Task<TQueryResult[]> QueryAsync<TQueryResult>(QueryContentGenre query, CancellationToken token)
        where TQueryResult: ContentGenre
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

        if (typeof(TQueryResult) == typeof(ContentGenre.WithName))
        {
            initSql += " JOIN genre ON genre.id = content_genre.genre_id ";
        }
        
        if (filters.Any())
        {
            initSql += $" WHERE {string.Join(" AND ", filters)}";
        }

        await using var connection = CreateConnection();
        var result = await connection.QueryAsync<TQueryResult>(initSql, parameters);
        return result.ToArray();
    }

    public Task<ContentGenre[]> QueryAsync(QueryContentGenre query, CancellationToken token)
    {
        throw new NotImplementedException();
    }

    public async Task<ContentGenre.WithName[]> GetByContentIdAsync(long contentId, CancellationToken token)
    {
        const string initSql = @"
SELECT * FROM content_genre cg
JOIN genre g on g.id = cg.genre_id
WHERE content_id = @ContentId;";
        
        var parameters = new
        {
            ContentId = contentId,
        };

        await using var connection = CreateConnection();
        var result = await connection.QueryAsync<ContentGenre.WithName>(
            initSql,
            parameters,
            commandType: CommandType.Text);
        
        return result.ToArray();
    }
}