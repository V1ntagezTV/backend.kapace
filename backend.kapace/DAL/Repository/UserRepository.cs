using backend.kapace.DAL.Experimental;
using backend.kapace.DAL.Models;
using backend.kapace.DAL.Models.Query;
using backend.kapace.DAL.Repository.Interfaces;
using backend.kapace.Infrastructure.Database;
using Dapper;
using Npgsql;

namespace backend.kapace.DAL.Repository;

public class UserRepository(NpgsqlDataSource npgsqlDataSource)
    : BaseKapaceRepository(npgsqlDataSource), IUserRepository
{
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

    public async Task<long> Insert(string nickname,
        string email,
        string passwordHash,
        DateTimeOffset createdAt,
        CancellationToken token)
    {
        const string initSql = """
                               INSERT INTO users(nickname, email, password_hash, created_at)
                               VALUES (@Nickname, @Email, @PasswordHash, @CreatedAt)
                               RETURNING id;
                               """;
        
        var parameters = new
        {
            Nickname = nickname,
            Email = email,
            PasswordHash = passwordHash,
            CreatedAt = createdAt
        };
        
        await using var connection = CreateConnection();
        var command = new CommandDefinition(initSql, parameters, cancellationToken: token);
        return await connection.QuerySingleAsync<long>(command);
    }

    public async Task UpdateVerifiedMail(long userId, bool isMailVerified, CancellationToken token)
    {
        const string initSql = $"""
                                UPDATE users
                                SET is_mail_verified = @{nameof(isMailVerified)}
                                WHERE id = @{nameof(userId)}
                                """;

        var parameters = new
        {
            userId,
            isMailVerified
        };
        
        await using var connection = CreateConnection();
        var command = new CommandDefinition(initSql, parameters, cancellationToken: token);
        await connection.ExecuteAsync(command);
    }

    public async Task UpdatePassword(long userId, string passwordHash, CancellationToken token)
    {
        const string initSql = $"""
                               UPDATE users
                               SET password_hash = @{nameof(passwordHash)}
                               WHERE id = @{nameof(userId)}
                               """;

        var parameters = new
        {
            userId,
            passwordHash
        };
        
        await using var connection = CreateConnection();
        var command = new CommandDefinition(initSql, parameters, cancellationToken: token);
        await connection.ExecuteAsync(command);
    }

    public async Task UpdateNickname(long userId, string newNickname, CancellationToken token)
    {
        const string initSql = $"""
                               UPDATE users
                               SET nickname = @{nameof(newNickname)}
                               WHERE id = @{nameof(userId)}
                               """;

        var parameters = new
        {
            userId,
            newNickname
        };
        
        await using var connection = CreateConnection();
        var command = new CommandDefinition(initSql, parameters, cancellationToken: token);
        await connection.ExecuteAsync(command);
    }

    public async Task EmailUpdate(long userId, string newEmail, CancellationToken token)
    {
        const string initSql = $"""
                                UPDATE users
                                SET email = @{nameof(newEmail)}
                                WHERE id = @{nameof(userId)}
                                """;

        var parameters = new { userId, newEmail };
        await using var connection = CreateConnection();
        var command = new CommandDefinition(initSql, parameters, cancellationToken: token);
        await connection.ExecuteAsync(command);
    }
}