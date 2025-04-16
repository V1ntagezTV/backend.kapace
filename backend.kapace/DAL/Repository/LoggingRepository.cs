using backend.Infrastructure.Database;
using backend.kapace.DAL.Models;
using backend.kapace.DAL.Repository.Interfaces;
using backend.kapace.Infrastructure.Database;
using Dapper;
using Newtonsoft.Json;
using Npgsql;

namespace backend.kapace.DAL.Repository;

public class LoggingRepository(NpgsqlDataSource npgsqlDataSource)
    : BaseKapaceRepository(npgsqlDataSource), ILoggingRepository
{
    public async Task Insert(LogModel logModel, CancellationToken token)
    {
        const string initSql = 
            "INSERT INTO logs (user_id, changes, created_at, metadata) VALUES (@UserId, @Changes, @CreatedAt, @Metadata)";

        var parameters = new
        {
            UserId = logModel.UserId,
            Changes = new JsonbParameter(JsonConvert.SerializeObject(logModel.Changes)),
            CreatedAt = logModel.CreatedAt,
            Metadata = logModel.Metadata
        };
        
        await using var connection = CreateConnection();
        var command = new CommandDefinition(initSql, parameters, cancellationToken: token);
        await connection.ExecuteAsync(command);
    }
}