using backend.kapace.BLL.Services.Interfaces;
using backend.kapace.Models.Requests.Episode;
using Microsoft.AspNetCore.Mvc;

namespace backend.kapace.Controllers;

[ApiController]
[Route("v1/episode")]
public class EpisodeController : Controller 
{
    private readonly IEpisodeService _episodeService;

    public EpisodeController(IEpisodeService episodeService)
    {
        _episodeService = episodeService;
    }
    
    [HttpPost("query")]
    public async Task<ActionResult> Query(V1EpisodeQueryRequest request, CancellationToken token)
    {
        var result = await _episodeService.QueryAsync(
            new EpisodeQuery
            {
                EpisodeIds = request.EpisodeIds,
                ContentIds = request.ContentIds,
                Limit = request.Limit,
                Offset = request.Offset
            },
            token);

        return Ok(result
            .Select(x => new
            {
                Id = x.Id,
                ContentId = x.ContentId,
                Title = x.Title,
                Image = x.Image,
                Number = x.Number,
                Views = x.Views,
                Stars = x.Stars,
                CreatedAt = x.CreatedAt,
                CreatedBy = x.CreatedBy
            })
            .ToArray());
    }
}