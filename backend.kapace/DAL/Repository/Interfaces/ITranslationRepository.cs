using backend.kapace.DAL.Models;

namespace backend.kapace.DAL.Repository.Interfaces;

public interface ITranslationRepository
{
    Task<IReadOnlyCollection<Translation>> GetByEpisodeAsync(long episodeId, CancellationToken token);
    Task<IReadOnlyCollection<Translation.WithTranslator>> GetByContentsAsync(long[] contentIds, CancellationToken token);
}