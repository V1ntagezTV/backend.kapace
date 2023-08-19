namespace backend.kapace.BLL.Services.Interfaces;

internal interface IChangesHistoryService
{
    Task ApproveAsync(long historyId, long userId, CancellationToken token);
    Task InsertChangesAsync(HistoryUnit historyUnit, CancellationToken token);
}