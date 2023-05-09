using backend.kapace.DAL.Models;

namespace backend.kapace.DAL.Repository.Interfaces;

public interface IGenreRepository
{
    Task<Genre[]> GetByIds(long[] genreIds, CancellationToken token);
}