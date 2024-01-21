using backend.kapace.BLL.Models;

namespace backend.kapace.BLL.Services.Interfaces;

public interface ITranslationService
{
    Task<IReadOnlyCollection<EpisodeTranslation>> GetByEpisodeAsync(
        long episodeId, 
        long? translatorId, 
        CancellationToken token);
}