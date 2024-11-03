using backend.kapace.BLL.Models.Genre;
using backend.kapace.BLL.Services.Interfaces;
using backend.kapace.DAL.Repository.Interfaces;
using backend.kapace.Models.Requests.Genre;
using Microsoft.AspNetCore.Mvc;

namespace backend.kapace.Controllers;

[ApiController]
[Route("v1/genre")]
public class GenreController : Controller 
{
    private readonly IGenreService _genreService;

    public GenreController(IGenreService genreService)
    {
        _genreService = genreService;
    }

    [HttpPost("query")]
    public async Task<ActionResult> V1Query(V1GenreQueryRequest parameters, CancellationToken token)
    {
        var query = new GenreQuery
        {
            Search = parameters.Search,
            GenreIds = parameters.GenreIds,
            Names = parameters.Names,
            Limit = parameters.Limit,
            Offset = parameters.Offset
        };

        var response = await _genreService.Query(query, token);

        return Ok(new
        {
            Genres = response.Select(g => new
            {
                Id = g.Id,
                Name = g.Name,
                CreatedAt = g.CreatedAt,
                CreatedBy = g.CreatedBy
            })
        });
    }
}