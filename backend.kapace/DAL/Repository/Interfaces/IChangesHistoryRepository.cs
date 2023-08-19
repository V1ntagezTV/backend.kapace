using backend.kapace.DAL.Models;

namespace backend.kapace.DAL.Repository.Interfaces;

internal interface IChangesHistoryRepository
{
    Task ApproveAsync(long historyId, long userId, DateTimeOffset approvedAt, CancellationToken token);
    Task<HistoryUnit> InsertAsync(HistoryUnit historyUnit, CancellationToken token);
    Task<IReadOnlyCollection<HistoryUnit>> QueryAsync(ChangesHistoryQuery query, CancellationToken token);
}