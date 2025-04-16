using backend.kapace.DAL.Models;

namespace backend.kapace.DAL.Repository.Interfaces;

public interface ILoggingRepository
{
    Task Insert(LogModel logModel, CancellationToken token);
}