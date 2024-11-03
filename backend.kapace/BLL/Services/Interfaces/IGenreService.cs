using backend.kapace.BLL.Models.Genre;

namespace backend.kapace.BLL.Services.Interfaces;

public interface IGenreService
{
    Task<Genre[]> Query(GenreQuery query, CancellationToken token);
}