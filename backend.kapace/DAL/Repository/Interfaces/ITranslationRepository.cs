using backend.kapace.DAL.Models;

namespace backend.kapace.DAL.Repository.Interfaces;

public interface ITranslationRepository
{
    Task<IReadOnlyCollection<Translation>> QueryAsync(
        long[]? contentIds, 
        long[]? episodeId, 
        long[]? translationId, 
        CancellationToken token);
    Task<IReadOnlyCollection<Translation>> GetByContentsAsync(long[] contentIds, CancellationToken token);
    Task<long> InsertAsync(InsertTranslation translation, CancellationToken token);
}