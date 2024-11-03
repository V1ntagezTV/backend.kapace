using backend.kapace.DAL.Models;

namespace backend.kapace.DAL.Repository.Interfaces;

public interface IContentGenreRepository
{
    Task Insert(ContentGenreV1[] contentGenres, CancellationToken token);
    
    Task<TQueryResult[]> QueryAsync<TQueryResult>(QueryContentGenre query, CancellationToken token)
        where TQueryResult : ContentGenreV1;

    Task<ContentGenreV1.WithName[]> GetByContentIdsAsync(long[] contentIds, CancellationToken token);
}