using backend.kapace.BLL.Models;
using backend.kapace.BLL.Models.EpisodeTranslations;

namespace backend.kapace.BLL.Services.Interfaces;

public interface ITranslationService
{
    Task<TranslatesView> GetByEpisodeAsync(
        long contentId,
        long? episodeId, 
        long? translatorId, 
        CancellationToken token);

    Task<EpisodeTranslation[]> QueryAsync(EpisodeTranslationQuery request, CancellationToken token);
}