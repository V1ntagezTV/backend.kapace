using backend.kapace.DAL.Experimental;
using backend.kapace.DAL.Models;
using backend.kapace.DAL.Models.Query;
using backend.kapace.DAL.Repository.Interfaces;
using backend.kapace.Infrastructure.Database;
using Dapper;
using Npgsql;

namespace backend.kapace.DAL.Repository;

public class GenreRepository : BaseKapaceRepository, IGenreRepository
{
    public GenreRepository(NpgsqlDataSource npgsqlDataSource) : base(npgsqlDataSource) { }

    public async Task<IEnumerable<Genre>> Query(GenreQuery query, CancellationToken token)
    {
        const string initSql = "SELECT * FROM genre WHERE 1 = 1";

        var command = new ExperimentalQueryBuilder(initSql)
            .WhereAny("id", query.GenreIds)
            .WhereAny("name", query.Names)
            .Like("name", query.Search)
            .AddPaging(query.Limit, query.Offset)
            .Build(token);

        await using var connection = CreateConnection();
        return await connection.QueryAsync<Genre>(command);
    }
}