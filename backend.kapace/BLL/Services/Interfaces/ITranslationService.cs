using backend.kapace.BLL.Models;
using backend.kapace.BLL.Models.EpisodeTranslations;
using backend.Models.Enums;

namespace backend.kapace.BLL.Services.Interfaces;

public interface ITranslationService
{
    Task<TranslatesView> GetByEpisodeAsync(
        long contentId,
        long? episodeId, 
        long? translatorId, 
        EpisodeOrderType? orderBy,
        CancellationToken token);

    Task<EpisodeTranslation[]> QueryAsync(EpisodeTranslationQuery request, CancellationToken token);
}