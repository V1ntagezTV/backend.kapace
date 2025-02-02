using backend.kapace.DAL.Experimental;
using backend.kapace.DAL.Models;
using backend.kapace.DAL.Models.Query;
using backend.kapace.DAL.Repository.Interfaces;
using backend.kapace.Infrastructure.Database;
using Dapper;
using Npgsql;

namespace backend.kapace.DAL.Repository;

public class UserPermissionRepository : BaseKapaceRepository, IUserPermissionRepository
{
    public UserPermissionRepository(NpgsqlDataSource npgsqlDataSource) : base(npgsqlDataSource) { }
    
    public async Task<IReadOnlyCollection<UserPermission>> Query(UserPermissionsQuery query, CancellationToken token)
    {
        const string initSql = @"SELECT * FROM users_permissions WHERE 1=1";

        var command = new ExperimentalQueryBuilder(initSql)
            .WhereAny("user_id", query.UserIds)
            .WhereAny("role_id", query.RoleIds)
            .Build(token);
        
        await using var connection = CreateConnection();
        var result = await connection.QueryAsync<UserPermission>(command);
        return result.ToArray();
    }
    
    public async Task Insert(UserPermission userPermission, CancellationToken token)
    {
        const string initSql = @"
INSERT INTO users_permissions(user_id, permission_id, gived_by, created_at)
VALUES(@UserId, @PermissionId, @GivedBy, @CreatedBy)";

        var parameters = new
        {
            UserId = userPermission.UserId,
            PermissionId = userPermission.PermissionId,
            GivedBy = userPermission.GivedBy,
            CreatedAt = userPermission.CreatedAt
        };
        
        await using var connection = CreateConnection();
        var command = new CommandDefinition(initSql, parameters, cancellationToken: token);
        await connection.ExecuteAsync(command);
    }
}