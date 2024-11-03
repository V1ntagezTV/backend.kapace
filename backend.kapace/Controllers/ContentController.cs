using backend.kapace.BLL.Models;
using backend.kapace.BLL.Services.Interfaces;
using backend.kapace.Models;
using backend.kapace.Models.Requests;
using backend.kapace.Models.Response;
using Microsoft.AspNetCore.Mvc;

namespace backend.kapace.Controllers;

[ApiController]
[Route("v1/content")]
public class ContentController : Controller
{
    private readonly IContentService _contentService;

    public ContentController(IContentService contentService)
    {
        _contentService = contentService;
    }

    [HttpPost("increment-views")]
    public async Task<ActionResult> V1IncrementViews(V1IncrementViewsRequest request, CancellationToken token)
    {
        await _contentService.IncrementViews(request.ContentId, token);

        return Ok();
    }

    [Obsolete("Используйте get-by-query")]
    [HttpPost("query")]
    public async Task<ActionResult<V1QueryResponse>> V1QueryAsync(V1QueryRequest request, CancellationToken token)
    {
        var response = await _contentService.QueryAsync(new ContentQuery
        {
            Ids = request.Ids,
            Limit = request.QueryPaging.Limit,
            Offset = request.QueryPaging.Offset,
        }, token);

        var content = response
            .Select(content => (QueryContent)content)
            .ToArray();

        return Ok(new V1QueryResponse(content));
    }

    [HttpPost("get-main-page-content")]
    public async Task<ActionResult<V1GetMainPageContentResponse>> V1GetMainPageContentAsync(
        V1GetMainPageContentRequest request, 
        CancellationToken token)
    {
        var contents = await _contentService.GetOrderByMapsAsync(
            request.MainPageTypes,
            new QueryPaging(request.Limit, request.Offset),
            token);

        var response = new V1GetMainPageContentResponse(
            contents
                .Select(pair => new V1GetMainPageContentResponse.V1GetMainPagePair(
                    pair.Key,
                    pair.Value.Select(contentWithGenres => new V1GetMainPageContentResponse.ContentInfo(
                            (QueryContent)contentWithGenres.ContentInfo,
                            contentWithGenres.Genres.Select(x => x.Name).ToArray()))
                        .ToArray()))
                .ToArray());
        
        return Ok(response);
    }

    [HttpPost("get-by-id")]
    public async Task<ActionResult<V1GetByIdResponse>> V1GetFullContentAsync(
        V1GetFullContentRequest request,
        CancellationToken token)
    {
        var data = await _contentService.GetFullAsync(
            request.ContentId,
            request.UserId,
            request.SelectedInfo,
            token);

        var episodes = data.Episodes
            .Select(x => new V1GetByIdResponse.V1GetFullContentEpisode(x.Id, x.Title, x.Image, x.Number))
            .ToArray();

        var genres = data.Genres
            .Select(x => new V1GetByIdResponse.V1GetFullContentGenre(x.GenreId, x.Name))
            .ToArray();

        return Ok(new V1GetByIdResponse((QueryContent)data.Content, episodes, genres, null));
    }

    [HttpPost("get-by-query")]
    public async Task<ActionResult<V1GetByQueryResponse>> V1GetByQueryAsync(V1GetByQueryRequest request, CancellationToken token)
    {
        var contents = await _contentService.GetByQueryAsync(
            request.Search,
            new SearchFilters()
            {
                ContentIds = request.Filters.ContentIds,
                Countries = request.Filters.Countries,
                ContentTypes = request.Filters.ContentTypes,
                ContentStatuses = request.Filters.ContentStatuses,
                GenreIds = request.Filters.GenreIds,
            },
            request.QueryPaging, 
            request.SelectedInfo,
            token);

        return Ok(new V1GetByQueryResponse
        {
            Content = contents.Select(response =>
            {
                var translactions = response.Translations.Select(x => new V1GetByQueryResponse.V1GetByQueryTranslation
                {
                    Id = x.Id,
                    EpisodeId = x.EpisodeId,
                    ContentId = x.ContentId,
                    Language = x.Language,
                    Link = x.Link,
                    TranslationType = x.TranslationType,
                    CreatedAt = x.CreatedAt,
                    CreatedBy = x.CreatedBy,
                    TranslatorId = x.TranslatorId,
                    TranslatorName = x.TranslatorName,
                    TranslatorLink = x.TranslatorLink,
                }).ToArray();

                var episodes = response.Episodes.Select(x => new V1GetByQueryResponse.V1GetByQueryEpisode
                {
                    Id = x.Id,
                    ContentId = x.ContentId,
                    Title = x.Title,
                    Image = x.Image,
                    Number = x.Number
                }).ToArray();

                var genres = response.Genres.Select(x => new V1GetByQueryResponse.V1GetByQueryGenre
                {
                    ContentId = x.ContentId,
                    GenreId = x.GenreId,
                    Name = x.Name,
                    CreatedAt = x.CreatedAt,
                    CreatedBy = x.CreatedBy
                }).ToArray();

                return new V1GetByQueryResponse.V1GetByQueryContent(
                    Content: (QueryContent)response,
                    Translations: translactions,
                    Episodes: episodes,
                    Genres: genres);
            }).ToArray()
        });
    }

    [HttpPost("search-by")]
    public async Task<ActionResult<V1RequestGetSearchByResponse>> GetSearchBy(V1RequestGetSearchByRequest request, CancellationToken token)
    {
        var response = await _contentService.SearchBy(request.Search, token);

        return Ok(new V1RequestGetSearchByResponse(
            response.Select(x => new SearchContentUnit(x.ContentId, x.Title, x.ImageId)).ToArray())
        );
    }
}