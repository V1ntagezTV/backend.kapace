using backend.kapace.DAL.Experimental;
using backend.kapace.DAL.Models;
using backend.kapace.DAL.Models.Query;
using backend.kapace.DAL.Repository.Interfaces;
using backend.kapace.Infrastructure.Database;
using Dapper;
using Npgsql;

namespace backend.kapace.DAL.Repository;

public class PermissionRepository : BaseKapaceRepository, IPermissionRepository
{
    public PermissionRepository(NpgsqlDataSource npgsqlDataSource) : base(npgsqlDataSource) { }
    
    public async Task<IReadOnlyCollection<Permission>> Query(PermissionQuery query, CancellationToken token)
    {
        const string initSql = @"SELECT * FROM permissions WHERE 1=1";

        var command = new ExperimentalQueryBuilder(initSql)
            .WhereAny("id", query.Ids)
            .WhereAny("alias", query.Aliases)
            .WhereAny("created_by", query.CreatedBy)
            .Build(token);
        
        await using var connection = CreateConnection();
        var result = await connection.QueryAsync<Permission>(command);
        return result.ToArray();
    }

    public async Task Insert(Permission permission, CancellationToken token)
    {
        const string initSql = @"
INSERT INTO permissions(alias, description, created_at, created_by)
VALUES(@Alias, @Description, @CreatedAt, @CreatedBy)";

        var parameters = new
        {
            
            Alias = permission.Alias,
            Description = permission.Description,
            CreatedBy = permission.CreatedBy,
            CreatedAt = permission.CreatedAt
        };
        
        await using var connection = CreateConnection();
        var command = new CommandDefinition(initSql, parameters, cancellationToken: token);
        await connection.ExecuteAsync(command);
    }
}