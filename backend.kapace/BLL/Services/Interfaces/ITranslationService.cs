using backend.kapace.BLL.Models;

namespace backend.kapace.BLL.Services.Interfaces;

public interface ITranslationService
{
    Task<IReadOnlyCollection<Translation>> GetByEpisodeAsync(long episodeId, CancellationToken token);
}