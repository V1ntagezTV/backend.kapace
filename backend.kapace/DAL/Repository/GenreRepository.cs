using backend.kapace.DAL.Models;
using backend.kapace.DAL.Repository.Interfaces;
using backend.kapace.Infrastructure.Database;
using Dapper;
using Npgsql;

namespace backend.kapace.DAL.Repository;

public class GenreRepository : BaseKapaceRepository, IGenreRepository
{
    public GenreRepository(NpgsqlDataSource npgsqlDataSource) : base(npgsqlDataSource) { }

    public async Task<Genre[]> GetByIds(long[] genreIds, CancellationToken token)
    {
        var initSql = @"SELECT * FROM content WHERE id = ANY(@{nameof(query.Ids)})";

        var parameters = new
        {
            Ids = genreIds,
        };

        await using var connection = CreateConnection();
        var result = await connection.QueryAsync<Genre>(initSql, parameters);
        return result.ToArray();
    }
}