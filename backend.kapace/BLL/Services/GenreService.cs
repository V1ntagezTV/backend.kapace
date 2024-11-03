using backend.kapace.BLL.Models.Genre;
using backend.kapace.BLL.Services.Interfaces;
using backend.kapace.DAL.Repository.Interfaces;

namespace backend.kapace.BLL.Services;

public class GenreService : IGenreService
{
    private readonly IGenreRepository _repository;

    public GenreService(IGenreRepository repository)
    {
        _repository = repository;
    }
    
    public async Task<Genre[]> Query(GenreQuery query, CancellationToken token)
    {
        var genres = await _repository.Query(new DAL.Models.Query.GenreQuery
        {
            Search = query.Search,
            GenreIds = query.GenreIds,
            Names = query.Names,
            Limit = query.Limit,
            Offset = query.Offset
        }, token);

        return genres
            .Select(g => new Genre(g.Id, g.Name, g.CreatedAt, g.CreatedBy))
            .ToArray();
    }
}