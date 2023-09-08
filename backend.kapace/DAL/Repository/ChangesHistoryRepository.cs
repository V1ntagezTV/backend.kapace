using backend.Infrastructure.Database;
using backend.kapace.DAL.Models;
using backend.kapace.DAL.Repository.Interfaces;
using backend.kapace.Infrastructure.Database;
using Dapper;
using Npgsql;

namespace backend.kapace.DAL.Repository;

internal class ChangesHistoryRepository : BaseKapaceRepository, IChangesHistoryRepository
{
    public ChangesHistoryRepository(NpgsqlDataSource npgsqlDataSource) : base(npgsqlDataSource) { }

    public async Task ApproveAsync(long id, long userId, DateTimeOffset approvedAt, CancellationToken token)
    {
        var sql = @"
            UPDATE changes_history 
            SET 
                approved_by = @UserId, 
                approved_at = @ApprovedAt
            WHERE id = @Id;";

        var parameters = new { Id = id, UserId = userId, ApprovedAt = approvedAt };

        await using var connection = CreateConnection();
        var command = new CommandDefinition(sql, parameters, cancellationToken: token);
        await connection.QueryAsync<Content>(command);
    }

    public async Task<IReadOnlyCollection<HistoryUnit>> QueryAsync(
        ChangesHistoryQuery query,
        CancellationToken token)
    {
        var sql = "select * from changes_history where ";
        var parameters = new DynamicParameters();
        var filters = new List<string>();

        if (query.Ids is {Length: > 0})
        {
            parameters.Add("@Ids", query.Ids);
            filters.Add("id = Any(@Ids)");
        }

        if (query.HistoryTypes is {Length: > 0})
        {
            var values = query.HistoryTypes.Cast<int>().ToArray();
            parameters.Add("@HistoryTypes", values);
            filters.Add("history_type = ANY(@HistoryTypes)");
        }

        if (query.TargetIds is {Length: > 0})
        {
            parameters.Add("@TargetIds", query.HistoryTypes);
            filters.Add("target_id = ANY(@TargetIds)");
        }

        if (filters.Any())
        {
            sql += string.Join(" AND ", filters);
        }

        await using var connection = CreateConnection();
        var command = new CommandDefinition(sql, parameters, cancellationToken: token);
        var result = await connection.QueryAsync<HistoryUnit>(command);

        return result.ToArray();
    }

    public async Task UpdateTextAsync(long historyId, string text, CancellationToken token)
    {
        const string sql = @"UPDATE changes_history SET text = @Text WHERE id = @HistoryId;";
        var parameters = new
        {
            HistoryId = historyId,
            Text = new JsonbParameter(text),
        };
        
        await using var connection = CreateConnection();
        var command = new CommandDefinition(sql, parameters, cancellationToken: token);
        await connection.QueryAsync(command);
    }

    public async Task<long> InsertAsync(
        HistoryUnit historyUnit,
        CancellationToken token)
    {
        const string columns = "target_id, history_type, text, created_by, created_at";
        
        const string sql = @$"
            INSERT INTO changes_history({columns})
            VALUES(@TargetId, @HistoryType, @Text, @CreatedBy, current_timestamp)
            RETURNING id;";

        var parameters = new
        {
            historyUnit.TargetId,
            historyUnit.HistoryType,
            Text = new JsonbParameter(historyUnit.Text),
            historyUnit.CreatedBy,
        };

        await using var connection = CreateConnection();
        var command = new CommandDefinition(sql, parameters, cancellationToken: token);
        return await connection.QueryFirstAsync<long>(command);
    }
}