using backend.kapace.BLL.Enums;
using backend.kapace.BLL.Exceptions;
using backend.kapace.BLL.Models;
using backend.kapace.BLL.Services.Interfaces;
using backend.kapace.DAL.Models;
using backend.kapace.DAL.Repository;
using backend.kapace.DAL.Repository.Interfaces;
using backend.kapace.Models;
using backend.Models.Enums;
using Dapper;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Content = backend.kapace.BLL.Models.VideoService.Content;
using ContentQuery = backend.kapace.BLL.Models.ContentQuery;
using Translation = backend.kapace.DAL.Models.Translation;

namespace backend.kapace.BLL.Services;

internal class ContentService : IContentService
{
    private readonly IContentRepository _contentRepository;
    private readonly IEpisodeRepository _episodeRepository;
    private readonly IContentGenreRepository _contentGenreRepository;
    private readonly ITranslationRepository _translationRepository;

    public ContentService(
        IContentRepository contentRepository,
        IEpisodeRepository episodeRepository,
        IContentGenreRepository contentGenreRepository,
        ITranslationRepository translationRepository)
    {
        _contentRepository = contentRepository;
        _episodeRepository = episodeRepository;
        _contentGenreRepository = contentGenreRepository;
        _translationRepository = translationRepository;
    }

    public async Task<Content[]> QueryAsync(ContentQuery query, CancellationToken token = default)
    {
        var dalQuery = new QueryContent
        {
            Ids = query.Ids,
            Countries = query.Countries.Select(x => (int)x).ToArray(),
            Limit = query.Limit,
            Offset = query.Offset,
        };

        var contents = await _contentRepository.QueryAsync(dalQuery, token);
        return contents.Select(Content.ToBLLContent).ToArray();
    }

    public async Task<IReadOnlyDictionary<MainPageType, Content[]>> GetOrderByMapsAsync(
        IReadOnlyCollection<MainPageType> pageTypes,
        QueryPaging pagingSettings,
        CancellationToken token)
    {
        var contentTasks = pageTypes.ToDictionary(
            pageType => pageType,
            pageType => GetByMainPageTypeAsync(pageType, pagingSettings, token));

        await Task.WhenAll(contentTasks.Values);

        return contentTasks.ToDictionary(
            x => x.Key,
            x => x.Value.Result.ToArray());
    }

    public async Task<FullContent> GetFullAsync(long contentId, long? userId, ContentSelectedInfo selectedInfo, CancellationToken token)
    {
        var contentQueryResult = await QueryAsync(new ContentQuery()
        {
            Ids = new[] { contentId }
        }, token);

        if (!contentQueryResult.Any())
        {
            throw new ContentNotFoundException(contentId);
        }

        var content = contentQueryResult.First();

        var episodes = selectedInfo.HasFlag(ContentSelectedInfo.Episodes)
            ? await _episodeRepository.QueryAsync(new QueryEpisode() { ContentIds = new[] {contentId}, }, token)
            : Array.Empty<Episode>();

        var contentGenres = selectedInfo.HasFlag(ContentSelectedInfo.ContentGenres)
            ? await _contentGenreRepository.GetByContentIdAsync(contentId, token)
            : Array.Empty<ContentGenre.WithName>();

        return new FullContent(
            content.Id,
            content.Title,
            content.Description,
            content.Type,
            content.Status,
            content.Image,
            content.ImportStars,
            content.OutSeries,
            content.PlannedSeries,
            content.Views,
            $"{(Country)content.Country}",
            content.ReleasedAt,
            content.CreatedAt,
            content.LastUpdateAt,
            content.MinAgeLimit,
            content.Duration,
            episodes
                .Select(x => new FullContent.FullContentEpisode(x.Id, x.Title, x.Image, x.Number))
                .ToArray(),
            contentGenres
                .Select(x => new FullContent.FullContentGenre(x.GenreId, x.Name))
                .ToArray(),
            null);
    }

    private async Task<IReadOnlyCollection<Content>> GetByMainPageTypeAsync(
        MainPageType mainPageType,
        QueryPaging pagingSettings,
        CancellationToken token)
    {
        const int popularListForDays = 14;
        var content = mainPageType switch
        {
            MainPageType.PopularForTwoWeeks => 
                await _contentRepository.GetOrderedByPopularAsync(popularListForDays, pagingSettings, token),
            
            MainPageType.LastCreated => 
                await _contentRepository.GetOrderedByNewestAsync(pagingSettings, token),
            
            MainPageType.LastUpdated => 
                await _contentRepository.GetOrderedByLastUpdatedAsync(pagingSettings, token),
            
            _ => Array.Empty<DAL.Models.Content>()
        };

        return content.Select(Content.ToBLLContent).ToArray();
    }

    public async Task<IReadOnlyCollection<GetByQueryResult>> GetByQueryAsync(
        string search, 
        SearchFilters searchFilters, 
        QueryPaging queryPaging, 
        ContentSelectedInfo selectedInfo,
        CancellationToken token)
    {
        var contentIds = Array.Empty<long>();
        var genresMapByContentIdTask = Task.FromResult(new Dictionary<long, GetByQueryResult.GetByQueryGenre[]>());
        var translatesMapByContentIdTask =
            Task.FromResult((IReadOnlyCollection<Translation.WithTranslator>)Array.Empty<Translation.WithTranslator>());
        var episodesMapByContentIdTask = Task.FromResult(Array.Empty<Episode>());
        
        Dictionary<long, GetByQueryResult.GetByQueryGenre[]> genresMapByContentId;

        if (searchFilters.GenreIds.Any())
        {
            genresMapByContentId = await GetByQueryGenresMapByContentIds(new QueryContentGenre()
            {
                GenreIds = searchFilters.GenreIds,
            }, token);

            contentIds = genresMapByContentId.Keys.ToArray();
        }

        var dalQuery = new QueryContent
        {
            Ids = contentIds,
            Countries = searchFilters.Countries.Select(x => (int)x).ToArray(),
            Statuses = searchFilters.ContentStatuses.Select(x => (int)x).ToArray(),
            Types = searchFilters.ContentTypes.Select(x => (int)x).ToArray(),
            Limit = queryPaging.Limit,
            Offset = queryPaging.Offset,
        };

        var contents = await _contentRepository.QueryAsync(dalQuery, token);
        if (!contents.Any())
        {
            return Array.Empty<GetByQueryResult>();
        }
        
        if (!string.IsNullOrEmpty(search))
        {
            contents = contents.Where(x => x.Title.Contains(search)).ToArray();
        }
        
        contentIds = contents.Select(x => x.Id).ToArray();
        
        if (selectedInfo.HasFlag(ContentSelectedInfo.ContentGenres) && !searchFilters.GenreIds.Any())
        {
            genresMapByContentIdTask = GetByQueryGenresMapByContentIds(new QueryContentGenre()
            {
                ContentIds = contentIds
            }, token);
        }
        
        if (selectedInfo.HasFlag(ContentSelectedInfo.Translations)) 
        {
            translatesMapByContentIdTask = _translationRepository.GetByContentsAsync(contentIds, token);
        }

        if (selectedInfo.HasFlag(ContentSelectedInfo.Episodes))
        {
            episodesMapByContentIdTask = _episodeRepository.QueryAsync(new QueryEpisode()
            {
                ContentIds = contentIds
            }, token);
        }
        
        await Task.WhenAll(translatesMapByContentIdTask, genresMapByContentIdTask, episodesMapByContentIdTask);

        var translatesMapByContentId = translatesMapByContentIdTask.Result
            .GroupBy(x => x.ContentId)
            .ToDictionary(
                group => group.Key,
                group => group
                    .Select(translate => new GetByQueryResult.GetByQueryTranslation
                    {
                        Id = translate.Id,
                        EpisodeId = translate.EpisodeId,
                        ContentId = translate.ContentId,
                        Language = (Language)translate.Language,
                        Link = translate.Link,
                        TranslationType = translate.TranslationType,
                        CreatedAt = translate.CreatedAt,
                        CreatedBy = translate.CreatedBy,
                        TranslatorId = translate.TranslatorId,
                        TranslatorName = translate.TranslatorName,
                        TranslatorLink = translate.TranslatorLink,
                    }).ToArray());
        
        var episodesMapByContentId = episodesMapByContentIdTask.Result
            .GroupBy(x => x.ContentId)
            .ToDictionary(
                group => group.Key,
                group => group
                    .Select(x => new GetByQueryResult.GetByQueryEpisode
                    {
                        Id = x.Id,
                        ContentId = x.ContentId,
                        Title = x.Title,
                        Image = x.Image,
                        Number = x.Number
                    }).ToArray());

        genresMapByContentId = genresMapByContentIdTask.Result;

        return contents.Select(x => new GetByQueryResult()
        {
            Id = x.Id,
            Title = x.Title,
            EngTitle = x.EngTitle,
            OriginTitle = x.OriginTitle,
            Description = x.Description,
            Type = (ContentType)x.Type,
            Status = (ContentStatus)x.Status,
            Image = x.Image,
            ImportStars = x.ImportStars,
            OutSeries = x.OutSeries,
            PlannedSeries = x.PlannedSeries,
            Views = x.Views,
            Country = (Country)x.Country,
            ReleasedAt = x.ReleasedAt,
            CreatedAt = x.CreatedAt,
            LastUpdateAt = x.LastUpdateAt,
            MinAgeLimit = x.MinAgeLimit,
            Duration = x.Duration,
            Genres = genresMapByContentId.TryGetValue(x.Id, out var genres)
                ? genres
                : Array.Empty<GetByQueryResult.GetByQueryGenre>(),
            Translations = translatesMapByContentId.TryGetValue(x.Id, out var translations)
                ? translations
                : Array.Empty<GetByQueryResult.GetByQueryTranslation>(),
            Episodes = episodesMapByContentId.TryGetValue(x.Id, out var contentEpisodes)
                ? contentEpisodes
                : Array.Empty<GetByQueryResult.GetByQueryEpisode>(),
        }).ToArray();
    }

    public async Task<long> InsertAsync(InsertContentModel model, CancellationToken token)
    {
        var id = await _contentRepository.InsertAsync(
            new InsertContentQuery
            {
                Image = model.Image,
                Title = model.Title,
                Description = model.Description,
                ContentType = (int)model.ContentType,
                Country = (int)model.Country,
                OriginTitle = model.OriginTitle,
                EngTitle = model.EngTitle,
                Status = (int?) model.Status ?? (int) ContentStatus.Null,
                Channel = model.Channel,
                MinAge = model.MinAge,
                Duration = model.Duration,
                PlannedSeries = model.PlannedSeries,
                ImportStars = 0,
                OutSeries = 0,
                Views = 0,
                ReleasedAt = model.ReleasedAt,
                CreatedAt = DateTimeOffset.Now,
                LastUpdatedAt = DateTimeOffset.Now
            }, token);

        return id;
    }

    private async Task<Dictionary<long, GetByQueryResult.GetByQueryGenre[]>> GetByQueryGenresMapByContentIds(
        QueryContentGenre queryContentGenre, 
        CancellationToken token)
    {
        var contentsByGenres = await _contentGenreRepository.QueryAsync<ContentGenre.WithName>(queryContentGenre, token);
        
        return contentsByGenres
            .GroupBy(x => x.ContentId)
            .ToDictionary(
                group => group.Key,
                group => group.Select(x => new GetByQueryResult.GetByQueryGenre
                {
                    ContentId = x.ContentId,
                    GenreId = x.GenreId,
                    Name = x.Name,
                    CreatedAt = x.CreatedAt,
                    CreatedBy = x.CreatedBy
                }).ToArray());
    }
}