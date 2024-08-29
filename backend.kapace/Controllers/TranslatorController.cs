using backend.kapace.BLL.Services;
using backend.kapace.BLL.Services.Interfaces;
using backend.kapace.Models.Requests.Translator;
using backend.kapace.Models.Response;
using Microsoft.AspNetCore.Mvc;

namespace backend.kapace.Controllers;

[ApiController]
[Route("v1/translator")]
public class TranslatorController : Controller
{
    private readonly ITranslatorService _translatorService;

    public TranslatorController(ITranslatorService translatorService) {
        _translatorService = translatorService;
    }

    [HttpPost("query")]
    public async Task<IActionResult> Query(TranslatorQueryRequest query, CancellationToken token) {
        var serviceQuery = new TranslatorQuery(query.TranslatorIds, query.Search, query.Offset, query.Limit);
        var translators = await _translatorService.Query(serviceQuery, token);

        var models = translators
            .Select(x => new TranslatorQueryResponse.Translator(x.TranslatorId, x.Name, x.Link))
            .ToArray();

        return Ok(new TranslatorQueryResponse(models));
    }
}
