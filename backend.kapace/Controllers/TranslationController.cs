using backend.kapace.BLL.Services.Interfaces;
using backend.kapace.Models.Requests;
using backend.kapace.Models.Response;
using Microsoft.AspNetCore.Mvc;

namespace backend.kapace.Controllers;

[ApiController]
[Route("v1/translations")]
public class TranslationController : Controller {
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
        var episode = await _translationService.GetByEpisodeAsync(request.EpisodeId, token);

        return new OkObjectResult(new V1GetByEpisodeResponse() {
            Translations = episode.Select(x => new V1GetByEpisodeResponse.V1GetByEpisodeTranslation
            {
                TranslationId = x.TranslationId,
                EpisodeId = x.EpisodeId,
                Language = x.Language,
                Link = x.Link,
                TranslationType = x.TranslationType,
                CreatedAt = x.CreatedAt,
                CreatedBy = x.CreatedBy,
                TranslatorId = x.TranslationId,
                Translator = "Азалии переводчик",
                TranslatorLink = "https://vk.com",
                Quality = 720,
            }).ToArray()
        });
    }
}
