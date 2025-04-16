using backend.kapace.DAL.Experimental;
using backend.kapace.DAL.Repository.Interfaces;
using backend.kapace.Infrastructure.Database;
using Dapper;
using Npgsql;

namespace backend.kapace.DAL.Repository;

public class VerificationCodeRepository : BaseKapaceRepository, IVerificationCodeRepository
{
    public VerificationCodeRepository(NpgsqlDataSource npgsqlDataSource) : base(npgsqlDataSource) { }

    public async Task<IReadOnlyCollection<VerificationCode>> Query(
        VerificationCodeQuery query,
        CancellationToken token)
    {
        const string sql = "SELECT * FROM verification_codes WHERE 1=1";
        var command = new ExperimentalQueryBuilder(sql)
            .Where("id", query.Id)
            .Where("user_id", query.UserId)
            .Where("type", query.Type)
            .Where("is_used", query.IsUsed)
            .Where("type", (int?)query.Type)
            .Custom("created_at > @CreatedAfter", nameof(query.CreatedAfter), query.CreatedAfter)
            .Build(token);

        await using var connection = CreateConnection();
        return (await connection.QueryAsync<VerificationCode>(command)).ToArray();
    }

    public async Task<long[]> Insert(IReadOnlyCollection<VerificationCode> insertModels, CancellationToken token)
    {
        const string sql = """
                           INSERT INTO verification_codes (user_id, code_hash, expires_at, type, attempts, email)
                           SELECT 
                               unnest(@UserIds),
                               unnest(@CodeHashes),
                               unnest(@ExpiresAts),
                               unnest(@Types),
                               unnest(@Attempts),
                               unnest(@Email)
                           RETURNING id;
                           """;

        var parameters = new
        {
            UserIds = insertModels.Select(x => x.UserId).ToArray(),
            CodeHashes = insertModels.Select(x => x.CodeHash).ToArray(),
            ExpiresAts = insertModels.Select(x => x.ExpiresAt).ToArray(),
            Types = insertModels.Select(x => x.Type).ToArray(),
            Email = insertModels.Select(x => x.Email).ToArray(),
            Attempts = insertModels.Select(x => x.Attempts).ToArray()
        };

        await using var connection = CreateConnection();
        var command = new CommandDefinition(sql, parameters, cancellationToken: token);
        return (await connection.QueryAsync<long>(command)).ToArray();
    }

    public async Task Update(VerificationCode updateModel, CancellationToken token)
    {
        const string sql = """
                           UPDATE verification_codes
                           SET
                               is_used = @IsUsed,
                               attempts = @Attempts
                           WHERE id = @Id;
                           """;

        var parameters = new
        {
            updateModel.Id,
            updateModel.IsUsed,
            updateModel.Attempts
        };

        await using var connection = CreateConnection();
        var command = new CommandDefinition(sql, parameters, cancellationToken: token);
        await connection.ExecuteAsync(command);
    }
    
    public async Task ClearTable(
        long userId, 
        TimeSpan olderThan, 
        VerificationCodeType codeType,
        CancellationToken token)
    {
        const string sql = "DELETE FROM verification_codes WHERE 1=1";
        var command = new ExperimentalQueryBuilder(sql)
            .Where("user_id", userId)
            .Where("type", (int)codeType)
            .Where("is_used", false)
            .Custom("created_at < NOW() - @olderThan", nameof(olderThan), olderThan)
            .Build(token);

        await using var connection = CreateConnection();
        await connection.ExecuteAsync(command);
    }
}







