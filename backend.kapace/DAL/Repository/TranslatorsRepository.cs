using backend.kapace.DAL.Models;
using backend.kapace.DAL.Models.Query;
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
        return await QueryAsync(new TranslatorQuery(ids, null, null, null), token);
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

    public async Task<IReadOnlyCollection<Translator>> QueryAsync(TranslatorQuery query, CancellationToken token)
    {
        var initSql = @"
            SELECT * FROM translator 
            WHERE 1 = 1";

        var parameters = new DynamicParameters();
        var filters = new List<string>();

        if (query.TranslatorIds?.Any() is true) {
            parameters.Add($"@{nameof(query.TranslatorIds)}", query.TranslatorIds);
            filters.Add("id = ANY(@Ids)");
        }

        if (!string.IsNullOrEmpty(query.Search)) {
            parameters.Add($"@{nameof(query.Search)}", query.Search);
            filters.Add("name ILIKE CONCAT('%', @Search, '%')");
        }

        if (filters.Any())
        {
            initSql += " AND " + string.Join(" AND ", filters);
        }

        if (query.Limit > 0) {
            var fieldName = $"@{nameof(query.Limit)}";
            parameters.Add(fieldName, query.Limit);
            initSql += $" LIMIT {fieldName}";
        }

        if (query.Offset > 0) {
            var fieldName = $"@{nameof(query.Offset)}";
            parameters.Add(fieldName, query.Offset);
            initSql += $" OFFSET {fieldName}";
        }

        await using var connection = CreateConnection();
        var command = new CommandDefinition(initSql, parameters, cancellationToken: token);
        var translators = await connection.QueryAsync<Translator>(command);

        return translators.ToArray();
    }
}
