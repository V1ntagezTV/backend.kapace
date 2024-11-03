using backend.kapace.BLL.Enums;
using backend.kapace.BLL.Exceptions;
using backend.kapace.BLL.Models;
using backend.kapace.BLL.Models.Genre;
using backend.kapace.BLL.Services.Interfaces;
using backend.kapace.DAL.Models;
using backend.kapace.DAL.Repository.Interfaces;
using backend.kapace.Models;
using backend.Models.Enums;
using Content = backend.kapace.BLL.Models.VideoService.Content;
using ContentQuery = backend.kapace.BLL.Models.ContentQuery;
using QueryContent = backend.kapace.DAL.Models.QueryContent;
using Translation = backend.kapace.DAL.Models.Translation;

namespace backend.kapace.BLL.Services;

public class ContentService : IContentService
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
        return contents.Select(Content.ToBllContent).ToArray();
    }

    public async Task<IReadOnlyDictionary<MainPageType, ContentWithGenres[]>> GetOrderByMapsAsync(
        IReadOnlyCollection<MainPageType> pageTypes,
        QueryPaging pagingSettings,
        CancellationToken token)
    {
        var contentTasks = pageTypes.ToDictionary(
            pageType => pageType,
            pageType => GetByMainPageTypeAsync(pageType, pagingSettings, token));

        await Task.WhenAll(contentTasks.Values);

        var contentIds = contentTasks.Values
            .SelectMany(c => c.Result.Select(x => x.Id))
            .Distinct()
            .ToArray();

        var contentGenresQuery = await _contentGenreRepository.GetByContentIdsAsync(contentIds, token);
        var contentGenresMap = contentGenresQuery
            .GroupBy(g => g.ContentId)
            .ToDictionary(g => g.Key);

        var contents = contentTasks.ToDictionary(
            keyValuePair => keyValuePair.Key,
            keyValuePair => keyValuePair.Value.Result
                .Select(content =>
                {
                    contentGenresMap.TryGetValue(content.Id, out var genres);
                    var contentGenres = (genres?.ToArray() ?? Array.Empty<ContentGenreV1.WithName>())
                        .Select(x => new Models.Genre.Genre(x.GenreId, x.Name, x.CreatedAt, x.CreatedBy))
                        .ToArray();

                    return new ContentWithGenres(content, contentGenres);
                })
                .ToArray());

        return contents;
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
            ? await _episodeRepository.QueryAsync(new QueryEpisode() { ContentIds = new[] { contentId }, }, token)
            : Array.Empty<DAL.Models.Episode>();

        var contentGenres = selectedInfo.HasFlag(ContentSelectedInfo.ContentGenres)
            ? await _contentGenreRepository.GetByContentIdsAsync(new []{contentId}, token)
            : Array.Empty<ContentGenreV1.WithName>();

        return new FullContent(
            content,
            episodes
                .Select(x => new FullContent.FullContentEpisode(x.Id, x.Title, x.ImageId, x.Number))
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

        return content.Select(Content.ToBllContent).ToArray();
    }

    public async Task<IReadOnlyCollection<GetByQueryResult>> GetByQueryAsync(
        string search,
        SearchFilters searchFilters,
        QueryPaging? queryPaging,
        ContentSelectedInfo selectedInfo,
        CancellationToken token)
    {
        var contentIds = Array.Empty<long>();
        if (searchFilters.ContentIds?.Any() == true)
        {
            contentIds = searchFilters.ContentIds;
        }
        
        var genresMapByContentIdTask = Task.FromResult(new Dictionary<long, GetByQueryResult.GetByQueryGenre[]>());
        var translatesMapByContentIdTask =
            Task.FromResult((IReadOnlyCollection<Translation>)Array.Empty<Translation>());
        var episodesMapByContentIdTask = Task.FromResult(Array.Empty<DAL.Models.Episode>());
        
        Dictionary<long, GetByQueryResult.GetByQueryGenre[]> genresMapByContentId;

        if (searchFilters.GenreIds.Any())
        {
            genresMapByContentId = await GetByQueryGenresMapByContentIds(new QueryContentGenre()
            {
                GenreIds = searchFilters.GenreIds,
            }, token);

            contentIds = genresMapByContentId.Keys.Intersect(contentIds).ToArray();
        }

        var dalQuery = new QueryContent
        {
            Ids = contentIds.ToArray(),
            Countries = searchFilters.Countries.Select(x => (int)x).ToArray(),
            Statuses = searchFilters.ContentStatuses.Select(x => (int)x).ToArray(),
            Types = searchFilters.ContentTypes.Select(x => (int)x).ToArray(),
            Limit = queryPaging?.Limit ?? 0,
            Offset = queryPaging?.Offset ?? 0,
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

        Dictionary<long, GetByQueryResult.GetByQueryTranslation[]> translatesMapByContentId = new();
        if (translatesMapByContentIdTask.Result.Any()) 
        {
            translatesMapByContentId = translatesMapByContentIdTask.Result
                .GroupBy(x => x.ContentId)
                .ToDictionary(
                    group => group.Key,
                    group => group
                        .Select(translate => new GetByQueryResult.GetByQueryTranslation
                        {
                            Id = translate.Id,
                            EpisodeId = translate.EpisodeId,
                            ContentId = translate.ContentId,
                            Language = translate.Language,
                            Link = translate.Link,
                            TranslationType = translate.TranslationType,
                            CreatedAt = translate.CreatedAt,
                            CreatedBy = translate.CreatedBy,
                            TranslatorId = translate.TranslatorId,
                            TranslatorName = translate.Name,
                            TranslatorLink = translate.TranslatorLink,
                        }).ToArray());
        }

        Dictionary<long, GetByQueryResult.GetByQueryEpisode[]> episodesMapByContentId = new();
        if (episodesMapByContentIdTask.Result.Any()) 
        {
            episodesMapByContentId = episodesMapByContentIdTask.Result
                            .GroupBy(x => x.ContentId)
                            .ToDictionary(
                                group => group.Key,
                                group => group
                                    .Select(x => new GetByQueryResult.GetByQueryEpisode
                                    {
                                        Id = x.Id,
                                        ContentId = x.ContentId,
                                        Title = x.Title,
                                        Image = x.ImageId,
                                        Number = x.Number
                                    }).ToArray());
        }


        genresMapByContentId = genresMapByContentIdTask.Result;

        return contents.Select(x => new GetByQueryResult()
        {
            Id = x.Id,
            Title = x.Title,
            EngTitle = x.EngTitle,
            OriginTitle = x.OriginTitle,
            Description = x.Description,
            ContentType = (ContentType)x.Type,
            Status = (ContentStatus)x.Status,
            ImageId = x.ImageId,
            ImportStars = x.ImportStars,
            OutSeries = x.OutSeries,
            PlannedSeries = x.PlannedSeries,
            Views = x.Views,
            Country = (Country)x.Country,
            ReleasedAt = x.ReleasedAt,
            CreatedAt = x.CreatedAt,
            LastUpdateAt = x.LastUpdateAt,
            MinAge = x.MinAgeLimit,
            Duration = x.Duration,
            Channel = x.Channel,
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

    public Task<IReadOnlyCollection<GetByQueryResult>> GetByQueryAsync(
        SearchFilters searchFilters, 
        ContentSelectedInfo selectedInfo, 
        CancellationToken token)
    {
        return GetByQueryAsync(search: null, searchFilters, queryPaging: null, selectedInfo, token);
    }

    public async Task<long> InsertAsync(InsertContentModel model, CancellationToken token)
    {
        var id = await _contentRepository.InsertAsync(
            new InsertContentQuery
            {
                Id = model.Id,
                ImageId = model.ImageId,
                Title = model.Title,
                Description = model.Description,
                ContentType = (int)model.ContentType,
                Country = (int)model.Country,
                OriginTitle = model.OriginTitle,
                EngTitle = model.EngTitle,
                Status = (int?) model.Status,
                Channel = model.Channel,
                MinAge = model.MinAge,
                Duration = model.Duration,
                PlannedSeries = model.PlannedSeries,
                ImportStars = 0,
                OutSeries = 0,
                Views = 0,
                ReleasedAt = model.ReleasedAt,
                CreatedAt = DateTimeOffset.UtcNow,
                LastUpdateAt = DateTimeOffset.UtcNow
            }, token);

        return id;
    }

    public async Task UpdateAsync(UpdateContentModel newContent, CancellationToken token)
    {
        await _contentRepository.UpdateAsync(new ContentUpdateQuery(newContent.ContentId)
        {
            ImageId = newContent.ImageId,
            Title = newContent.Title,
            EngTitle = newContent.EngTitle,
            OriginTitle = newContent.OriginalTitle,
            Description = newContent.Description,
            Country = newContent.Country,
            ContentType = newContent.ContentType,
            Duration = newContent.Duration,
            ReleasedAt = newContent.ReleasedAt,
            PlannedSeries = newContent.PlannedSeries,
            MinAge = newContent.MinAge
        }, token);
    }
    
    public async Task<SearchContentUnit[]> SearchBy(string? search, CancellationToken token)
    {
        if (string.IsNullOrEmpty(search))
        {
            return Array.Empty<SearchContentUnit>();
        }
        
        var result = new List<SearchContentUnit>();
        if (long.TryParse(search, out var contentId))
        {
            var queryResponse = await _contentRepository.QueryAsync(new QueryContent
            {
                Ids = new[] { contentId }
            }, token);

            result.AddRange(queryResponse.Select(x => new SearchContentUnit(x.Id, x.Title, x.ImageId)).ToArray());
        }

        var searchByText = await _contentRepository.SearchByText(search, token);
        result.AddRange(searchByText.Select(x => new SearchContentUnit(x.Id, x.Title, x.ImageId)).ToArray());

        return result.ToArray();
    }

    public async Task IncrementViews(long contentId, CancellationToken token)
    {
        await _contentRepository.IncrementViews(contentId, token);
    }

    private async Task<Dictionary<long, GetByQueryResult.GetByQueryGenre[]>> GetByQueryGenresMapByContentIds(
        QueryContentGenre queryContentGenre, 
        CancellationToken token)
    {
        var contentsByGenres = await _contentGenreRepository.QueryAsync<ContentGenreV1.WithName>(queryContentGenre, token);
        
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