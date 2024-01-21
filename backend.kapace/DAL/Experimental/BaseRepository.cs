using System.Collections;
using backend.kapace.Infrastructure.Database;
using Dapper;
using Npgsql;

namespace backend.kapace.DAL.Experimental;

// CRUD
public abstract class BaseRepository<T> : BaseKapaceRepository where T: BaseDataColumns
{
    private const int QueryTimeoutSeconds = 5; 
    private readonly string _tableName;
    

    protected BaseRepository(string tableName, NpgsqlDataSource npgsqlDataSource) : base(npgsqlDataSource)
    {
        _tableName = tableName;
    }

    public async Task<T[]> InsertAsync(T[] models, CancellationToken token)
    {
        if (!models.Any())
        {
            return Array.Empty<T>();
        }

        var columnNames = models.First().GetColumnNames();
        var sqlColumnNames = string.Join(", ", columnNames);
        var sqlColumnValueNames = string.Join(", ", columnNames.Select(x => $"@{x}"));

        var initSql = @$"INSERT INTO {_tableName}({sqlColumnNames}) VALUES ({sqlColumnValueNames})";

        await using var connection = CreateConnection();
        var command = new CommandDefinition(initSql, models, cancellationToken: token, commandTimeout: QueryTimeoutSeconds);
        var result = await connection.QueryAsync<T>(command);

        return result?.ToArray() ?? Array.Empty<T>();
    }

    public async Task<T[]> SelectAsync(BaseQuery query, CancellationToken token)
    {
        var initSql = @$"SELECT * FROM {_tableName}";
        var parameters = new DynamicParameters();
        if (query.WhereFilters.Any())
        {
            var filters = query.WhereFilters.Select(x =>
            {
                parameters.Add(x.Key, x.Value);
                
                if (x.Value is IEnumerable)
                {
                    return $"@{x.Key} = ANY({x.Value})";
                }
                
                return $"@{x.Key} = {x.Value}";
            });
            initSql += $" WHERE @{string.Join(" AND ", filters)}";
        }

        if (query.OrderBy.Any())
        {
            initSql += $" ORDER BY @{string.Join(", ", query.OrderBy)}";
        }

        await using var connection = CreateConnection();
        var command = new CommandDefinition(
            initSql,
            parameters,
            cancellationToken: token,
            commandTimeout: QueryTimeoutSeconds
        );
        var result = await connection.QueryAsync<T>(command);

        return result?.ToArray() ?? Array.Empty<T>();
    }
}