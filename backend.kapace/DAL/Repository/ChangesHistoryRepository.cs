﻿using backend.kapace.DAL.Models;
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

        if (query.Ids.Any())
        {
            parameters.Add("@Ids", query.Ids);
            filters.Add("id = Any(@Ids)");
        }

        if (query.HistoryTypes.Any())
        {
            parameters.Add("@HistoryTypes", query.HistoryTypes);
            filters.Add("history_type = ANY(@HistoryTypes)");
        }

        if (query.TargetIds.Any())
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
        const string sql = @"UPDATE changes_history WHERE id = @HistoryId SET text = @Text;";
        
        var parameters = new
        {
            HistoryId = historyId,
            Text = text
        };
        
        await using var connection = CreateConnection();
        var command = new CommandDefinition(sql, parameters, cancellationToken: token);
        await connection.QueryAsync(command);
    }

    public async Task<HistoryUnit> InsertAsync(
        HistoryUnit historyUnit,
        CancellationToken token)
    {
        const string columns = "id, target_id, history_type, text, created_by, created_at";
        
        const string sql = @$"
            INSERT INTO changes_history({columns})
            VALUES(@Id, @TargetId, @HistoryType, @Text, @CreatedBy, current_timestamp);";

        var parameters = new
        {
            historyUnit.Id,
            historyUnit.TargetId,
            historyUnit.HistoryType,
            historyUnit.Text,
            historyUnit.CreatedBy,
        };

        await using var connection = CreateConnection();
        var command = new CommandDefinition(sql, parameters, cancellationToken: token);
        return await connection.QueryFirstAsync<HistoryUnit>(command);
    }
}