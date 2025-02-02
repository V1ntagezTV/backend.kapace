using backend.kapace.DAL.Experimental;
using backend.kapace.DAL.Models;
using backend.kapace.DAL.Models.Query;
using backend.kapace.DAL.Repository.Interfaces;
using backend.kapace.Infrastructure.Database;
using Dapper;
using Npgsql;

namespace backend.kapace.DAL.Repository;

public class UserRepository : BaseKapaceRepository, IUserRepository
{
    public UserRepository(NpgsqlDataSource npgsqlDataSource) : base(npgsqlDataSource) { }

    public async Task<IReadOnlyCollection<User>> Query(UserQuery query, CancellationToken token)
    {
        const string initSql = "SELECT * FROM users WHERE 1=1";

        var command = new ExperimentalQueryBuilder(initSql)
            .WhereAny("nickname", query.Nicknames)
            .WhereAny("email", query.Emails)
            .WhereAny("id", query.UserIds)
            .Build(token);

        await using var connection = CreateConnection();
        var result = await connection.QueryAsync<User>(command);

        return result.ToArray();
    }

    public async Task Insert(
        string nickname,
        string email,
        string passwordHash,
        DateTimeOffset createdAt,
        CancellationToken token)
    {
        const string initSql = @"
INSERT INTO users(nickname, email, password_hash, created_at)
VALUES (@Nickname, @Email, @PasswordHash, @CreatedAt);";
        
        var parameters = new
        {
            Nickname = nickname,
            Email = email,
            PasswordHash = passwordHash,
            CreatedAt = createdAt
        };
        
        await using var connection = CreateConnection();
        var command = new CommandDefinition(initSql, parameters, cancellationToken: token);
        await connection.ExecuteAsync(command);
    }
}