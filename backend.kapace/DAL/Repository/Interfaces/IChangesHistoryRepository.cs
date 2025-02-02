using backend.kapace.DAL.Models;
using Npgsql;

namespace backend.kapace.DAL.Repository.Interfaces;

public interface IChangesHistoryRepository
{
    Task ApproveAsync(long historyId, long userId, DateTimeOffset approvedAt, CancellationToken token);
    Task<long> InsertAsync(HistoryUnit historyUnit, CancellationToken token);
    Task<IReadOnlyCollection<HistoryUnit>> QueryAsync(ChangesHistoryQuery query, CancellationToken token);
    Task UpdateTextAsync(long historyId, string text, CancellationToken token);
    Task<NpgsqlTransaction> BeginTransaction(CancellationToken token);
    Task UpdateTargetId(long historyId, long newTargetId, CancellationToken token);
}