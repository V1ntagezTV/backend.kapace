namespace backend.kapace.BLL.Services.Interfaces;

public interface IChangesHistoryService
{
    Task ApproveAsync(long historyId, long userId, CancellationToken token);
    Task<long> InsertChangesAsync(HistoryUnit historyUnit, CancellationToken token);
    Task UpdateImageAsync(long historyId, long imageId, CancellationToken token);
    Task<HistoryUnit[]> QueryAsync(long[] ids, CancellationToken token);
}