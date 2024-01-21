using backend.kapace.DAL.Models;
using backend.kapace.DAL.Repository.Interfaces;
using backend.kapace.Infrastructure.Database;
using Dapper;
using Npgsql;

namespace backend.kapace.DAL.Repository;

public class TranslatorsRepository : BaseKapaceRepository, ITranslatorsRepository  
{
    public TranslatorsRepository(NpgsqlDataSource npgsqlDataSource) : base(npgsqlDataSource) { }

    public async Task<IReadOnlyCollection<Translator>> QueryAsync(long[] ids, CancellationToken token)
    {
        const string initSql = @"SELECT * FROM translator WHERE id = ANY(@Ids);";

        var parameters = new {
            Ids = ids
        };

        await using var connection = CreateConnection();
        var command = new CommandDefinition(initSql, parameters, cancellationToken: token);
        var translators = await connection.QueryAsync<Translator>(command);

        return translators.ToArray();
    }

    public async Task<long> InsertAsync(Translator translator, CancellationToken token)
    {
        const string initSql = @"
            INSERT INTO translator(
                name,
                link
            )
            VALUES (
                @Name,
                @Link
            )
            RETURNING id;";

        var parameters = new 
        {
            translator.Name,
            translator.Link
        };

        await using var connection = CreateConnection();
        var command = new CommandDefinition(initSql, parameters, cancellationToken: token);
        var id = await connection.QueryFirstAsync<long>(command);

        return id;
    }
}
