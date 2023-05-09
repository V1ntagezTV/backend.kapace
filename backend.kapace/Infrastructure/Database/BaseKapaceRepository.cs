using Npgsql;

namespace backend.kapace.Infrastructure.Database;

public class BaseKapaceRepository
{
    private readonly NpgsqlDataSource _npgsqlDataSource;

    protected BaseKapaceRepository(NpgsqlDataSource npgsqlDataSource)
    {
        _npgsqlDataSource = npgsqlDataSource;
    }

    protected NpgsqlConnection CreateConnection() => _npgsqlDataSource.CreateConnection();
}