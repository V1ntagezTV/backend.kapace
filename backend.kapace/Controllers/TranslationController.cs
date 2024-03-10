using backend.kapace.BLL.Services.Interfaces;
using backend.kapace.Models.Requests;
using backend.kapace.Models.Response;
using Microsoft.AspNetCore.Mvc;

namespace backend.kapace.Controllers;

[ApiController]
[Route("v1/translations")]
public class TranslationController : Controller 
{
    private readonly ITranslationService _translationService;

    public TranslationController(ITranslationService translationService)
    {
        _translationService = translationService;
    }

    [HttpPost("get-by-episode")]
    public async Task<ActionResult<V1GetByEpisodeResponse>> GetByEpisodeAsync(
        V1GetByEpisodeRequest request,
        CancellationToken token)
    {
        var episodeTranslations = await _translationService.GetByEpisodeAsync(
            request.ContentId,
            request.EpisodeId, 
            request.TranslatorId,
            token);

        return new OkObjectResult(new V1GetByEpisodeResponse() {
            Translations = episodeTranslations.Select(x => new V1GetByEpisodeResponse.V1GetByEpisodeTranslation
            {
                TranslationId = x.Id,
                EpisodeId = x.EpisodeId,
                Language = x.Language,
                Link = x.Link,
                TranslationType = x.TranslationType,
                CreatedAt = x.CreatedAt,
                CreatedBy = x.CreatedBy,
                Quality = x.Quality,
                TranslatorId = x.Translator?.TranslatorId,
                Translator = x.Translator?.Name,
                TranslatorLink = x.Translator?.Link,
                Views = x.Episode?.Views ?? 0,
                Stars = x.Episode?.Stars ?? 0,
                Number = x.Episode?.Number ?? 0,
                Title = x.Episode?.Title ?? "",
            }).ToArray()
        });
    }
}
