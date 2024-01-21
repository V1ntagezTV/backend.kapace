using backend.kapace.DAL.Models;

namespace backend.kapace.BLL.Services.Interfaces;

public interface IEpisodeService
{
    Task InsertAsync(Episode episode, CancellationToken token);
    Task UpdateAsync(Episode episode, CancellationToken token);
}
