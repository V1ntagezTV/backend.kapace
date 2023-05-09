using backend.kapace.DAL.Models;

namespace backend.kapace.DAL.Repository.Interfaces;

public interface IContentGenreRepository
{
    Task<TQueryResult[]> QueryAsync<TQueryResult>(QueryContentGenre query, CancellationToken token)
        where TQueryResult : ContentGenre;

    Task<ContentGenre.WithName[]> GetByContentIdAsync(long contentId, CancellationToken token);
}