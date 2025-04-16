using Npgsql;

namespace backend.kapace.Infrastructure.Database;

public class BaseKapaceRepository
{
    protected static readonly int DefaultTimeoutInSeconds = 5;
    private readonly NpgsqlDataSource _npgsqlDataSource;

    protected BaseKapaceRepository(NpgsqlDataSource npgsqlDataSource)
    {
        _npgsqlDataSource = npgsqlDataSource;
    }

    protected NpgsqlConnection CreateConnection() => _npgsqlDataSource.CreateConnection();

    public async Task<NpgsqlTransaction> BeginTransaction(CancellationToken token) 
    {
        using var conn = CreateConnection();
        await conn.OpenAsync(token);
        return await conn.BeginTransactionAsync(System.Data.IsolationLevel.ReadCommitted, token);
    }
}