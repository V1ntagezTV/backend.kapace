using backend.kapace.BLL.Models;
using backend.kapace.BLL.Services.Interfaces;
using backend.kapace.DAL.Repository.Interfaces;

namespace backend.kapace.BLL.Services;

public class TranslationService : ITranslationService
{
    private readonly ITranslationRepository _translationRepository;

    public TranslationService(ITranslationRepository translationRepository)
    {
        _translationRepository = translationRepository;
    }
    
    public async Task<IReadOnlyCollection<Translation>> GetByEpisodeAsync(long episodeId, CancellationToken token)
    {
        var episodes = await _translationRepository.GetByEpisodeAsync(episodeId, token);
        if (!episodes.Any())
        {
            return Array.Empty<Translation>();
        }

        return episodes
            .Select(x => new Translation(x.Id, x.EpisodeId, x.Language, x.Link, x.TranslationType, x.CreatedAt, x.CreatedBy))
            .ToArray();
    }
}