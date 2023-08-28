using backend.kapace.BLL.Enums;
using backend.kapace.DAL.Models;
using backend.kapace.Infrastructure.Database;
using Dapper;
using Npgsql;

namespace backend.kapace.DAL.Repository;

public class StaticFilesRepository : BaseKapaceRepository, IStaticFilesRepository
{
    private const string TableName = "static_files";
    
    public StaticFilesRepository(NpgsqlDataSource npgsqlDataSource) : base(npgsqlDataSource) { }

    public async Task<long> InsertAsync(
        string fileName,
        long linkId,
        StaticFileLinkType linkType,
        DateTimeOffset createdAt,
        CancellationToken token)
    {
        const string initSql =
            @$"INSERT INTO {TableName}(file_name, link_type, link_id, created_at) 
               VALUES (@FileName, @LinkType, @LinkId, @CreatedAt)
               RETURNING id;";

        var parameters = new
        {
            FileName = fileName,
            LinkType = (int)linkType,
            LinkId = linkId,
            CreatedAt = createdAt,
        };

        await using var connection = CreateConnection();
        var command = new CommandDefinition(initSql, parameters, cancellationToken: token);
        return await connection.QuerySingleAsync<long>(command);
    }

    public async Task<StaticFile> GetByIdAsync(long imageId, CancellationToken token)
    {
        const string initSql = $@"SELECT * FROM {TableName} WHERE id = @ImageId;";
        
        await using var connection = CreateConnection();
        var command = new CommandDefinition(initSql, new {ImageId = imageId}, cancellationToken: token);
        var result = await connection.QueryFirstAsync<StaticFile>(command);

        return result;
    }
}