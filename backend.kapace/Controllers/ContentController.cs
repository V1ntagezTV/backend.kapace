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

    [HttpPost("query")]
    public async Task<ActionResult<V1QueryResponse>> V1QueryAsync(V1QueryRequest request, CancellationToken token)
    {
        var response = await _contentService.QueryAsync(new ContentQuery
        {
            Ids = request.Ids,
            Limit = request.QueryPaging.Limit,
            Offset = request.QueryPaging.Offset,
        }, token);

        return new OkObjectResult(new V1QueryResponse(response.Select(x => new V1QueryResponse.Content
        {
            Id = x.Id,
            Title = x.Title,
            Description = x.Description,
            Type = x.Type,
            Status = x.Status,
            Image = x.Image,
            ImportStars = x.ImportStars,
            OutSeries = x.OutSeries,
            PlannedSeries = x.PlannedSeries,
            Views = x.Views,
            Country = x.Country,
            ReleasedAt = x.ReleasedAt,
            CreatedAt = x.CreatedAt,
            LastUpdateAt = x.LastUpdateAt,
            MinAgeLimit = x.MinAgeLimit,
            Duration = x.Duration
        }).ToArray()));
    }

    [HttpPost("get-main-page-content")]
    public async Task<ActionResult<V1GetMainPageContentResponse>> V1GetMainPageContentAsync(
        V1GetMainPageContentRequest request, 
        CancellationToken token)
    {
        var response = await _contentService.GetOrderByMapsAsync(
            request.MainPageTypes,
            new QueryPaging(request.Limit, request.Offset),
            token);

        return new OkObjectResult(
            new V1GetMainPageContentResponse(
                response.Select(x => new V1GetMainPageContentResponse.V1GetMainPagePair
                {
                    MainPageType = x.Key,
                    Content = x.Value.Select(x =>
                        new V1GetMainPageContentResponse.V1GetMainPageContent
                        {
                            Id = x.Id,
                            Title = x.Title,
                            Image = x.Image,
                            Views = x.Views,
                            ImportStars = x.ImportStars,
                            SeriesOut = x.OutSeries,
                            SeriesPlanned = x.PlannedSeries
                        }).ToArray()
                }).ToArray())
            );
    }

    [HttpPost("get-by-id")]
    public async Task<ActionResult<V1GetFullContentResponse>> V1GetFullContentAsync(
        V1GetFullContentRequest request,
        CancellationToken token)
    {
        var content = await _contentService.GetFullAsync(
            request.ContentId,
            request.UserId,
            request.SelectedInfo,
            token);

        return new OkObjectResult(new V1GetFullContentResponse(
            content.ContentId,
            content.Title,
            content.Description,
            content.Type,
            content.Status,
            content.Image,
            content.ImportStars,
            content.OutSeries,
            content.PlannedSeries,
            content.Views,
            content.Country,
            content.ReleasedAt,
            content.CreatedAt,
            content.LastUpdateAt,
            content.MinAgeLimit,
            content.Duration,
            content.Episodes
                .Select(x => new V1GetFullContentResponse.V1GetFullContentEpisode(x.Id, x.Title, x.Image, x.Number))
                .ToArray(),
            content.Genres
                .Select(x => new V1GetFullContentResponse.V1GetFullContentGenre(x.GenreId, x.Name))
                .ToArray(),
            null));
    }

    [HttpPost("get-by-query")]
    public async Task<ActionResult<V1GetByQueryResponse>> V1GetByQueryAsync(V1GetByQueryRequest request, CancellationToken token)
    {
        var contents = await _contentService.GetByQueryAsync(
            request.Search,
            new SearchFilters()
            {
                Countries = request.Filters.Countries,
                ContentTypes = request.Filters.ContentTypes,
                ContentStatuses = request.Filters.ContentStatuses,
                GenreIds = request.Filters.GenreIds,
            },
            request.QueryPaging, 
            request.SelectedInfo,
            token);

        return new OkObjectResult(new V1GetByQueryResponse()
        {
            Content = contents.Select(x => new V1GetByQueryResponse.V1GetByQueryContent
            {
                Id = x.Id,
                Title = x.Title,
                EngTitle = x.EngTitle,
                OriginTitle = x.OriginTitle,
                Description = x.Description,
                Type = x.Type,
                Status = x.Status,
                Image = x.Image,
                ImportStars = x.ImportStars,
                OutSeries = x.OutSeries,
                PlannedSeries = x.PlannedSeries,
                Views = x.Views,
                Country = x.Country,
                ReleasedAt = x.ReleasedAt,
                CreatedAt = x.CreatedAt,
                LastUpdateAt = x.LastUpdateAt,
                MinAgeLimit = x.MinAgeLimit,
                Duration = x.Duration,
                Translations = x.Translations.Select(x => new V1GetByQueryResponse.V1GetByQueryTranslation
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
                }).ToArray(),
                Episodes = x.Episodes.Select(x => new V1GetByQueryResponse.V1GetByQueryEpisode
                {
                    Id = x.Id,
                    ContentId = x.ContentId,
                    Title = x.Title,
                    Image = x.Image,
                    Number = x.Number
                }).ToArray(),
                Genres = x.Genres.Select(x => new V1GetByQueryResponse.V1GetByQueryGenre
                {
                    ContentId = x.ContentId,
                    GenreId = x.GenreId,
                    Name = x.Name,
                    CreatedAt = x.CreatedAt,
                    CreatedBy = x.CreatedBy
                }).ToArray(),
            }).ToArray()
        });
    }
}