using System.ComponentModel.DataAnnotations;
using backend.kapace.BLL.Models.EpisodeTranslations;
using backend.kapace.BLL.Services.Interfaces;
using backend.kapace.Models.Requests;
using backend.kapace.Models.Requests.EpisodeTranslation;
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

    [HttpPost("query")]
    public async Task<ActionResult> Query(EpisodeTranslationQueryRequest request, CancellationToken token)
    {
        var query = new EpisodeTranslationQuery
        {
            EpisodeTranslationIds = request.EpisodeTranslationIds,
            TranslatorIds = request.TranslatorIds,
            ContentIds = request.ContentIds,
            EpisodeIds = request.EpisodeIds,
            Limit = request.Limit,
            Offset = request.Offset
        };
        
        var x = await _translationService.QueryAsync(query, token);

        return Ok(x.Select(translation => new
            {
                Id = translation.Id,
                ContentId = translation.ContentId,
                EpisodeId = translation.EpisodeId,
                Link = translation.Link,
                TranslationType = translation.TranslationType,
                CreatedAt = translation.CreatedAt,
                CreatedBy = translation.CreatedBy,
                Quality = translation.Quality,
                TranslatorId = translation.TranslatorId,
                Language = translation.Language
            }));
    }

    [HttpPost("get-by-episode")]
    public async Task<ActionResult<V1GetByEpisodeResponse>> GetByEpisodeAsync(
        V1GetByEpisodeRequest request,
        CancellationToken token)
    {
        var response = await _translationService.GetByEpisodeAsync(
            request.ContentId,
            request.EpisodeId, 
            request.TranslatorId,
            token);

        var translators = response.Translators
            .Select(translator => new V1GetByEpisodeResponse.Translator(translator.Id, translator.Name))
            .ToArray();

        var episodes = response.Episodes
            .Select(episode => new V1GetByEpisodeResponse.Episode(
                Id: episode.Id,
                episode.Title,
                episode.Number,
                episode.Views,
                episode.Stars,
                episode.Translations
                    .Select(translation => new V1GetByEpisodeResponse.EpisodeTranslation(
                        translation.Id,
                        translation.EpisodeId,
                        translation.Lang,
                        translation.Link,
                        translation.TranslationType,
                        translation.CreatedAt,
                        translation.CreatedBy,
                        translation.Quality))
                    .ToArray()))
            .ToArray();

        return new OkObjectResult(new V1GetByEpisodeResponse(translators, episodes));
    }
}