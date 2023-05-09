using Translation = backend.kapace.DAL.Models.Translation;

namespace backend.kapace.DAL.Repository.Interfaces;

public interface ITranslationRepository
{
    Task<IReadOnlyCollection<Translation>> GetByEpisodeAsync(long episodeId, CancellationToken token);
    Task<IReadOnlyCollection<Translation>> GetByContentsAsync(long[] contentIds, CancellationToken token);
}