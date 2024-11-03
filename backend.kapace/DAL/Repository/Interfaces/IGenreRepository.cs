using backend.kapace.DAL.Models;
using backend.kapace.DAL.Models.Query;

namespace backend.kapace.DAL.Repository.Interfaces;

public interface IGenreRepository
{
    Task<IEnumerable<Genre>> Query(GenreQuery query, CancellationToken token);
}