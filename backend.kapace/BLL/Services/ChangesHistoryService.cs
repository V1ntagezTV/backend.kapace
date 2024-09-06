using backend.kapace.BLL.Enums;
using backend.kapace.BLL.Exceptions;
using backend.kapace.BLL.Extensions;
using backend.kapace.BLL.Mapper.ChangesHistory;
using backend.kapace.BLL.Models;
using backend.kapace.BLL.Services.Interfaces;
using backend.kapace.Controllers;
using backend.kapace.DAL.Models;
using backend.kapace.DAL.Repository.Interfaces;
using Newtonsoft.Json;
using static backend.kapace.BLL.Exceptions.ChangesHistoryService;
using Content = backend.kapace.BLL.Models.VideoService.Content;
using HistoryUnit = backend.kapace.BLL.Models.HistoryChanges.HistoryUnit;

namespace backend.kapace.BLL.Services;

public class ChangesHistoryService : IChangesHistoryService
{
    private static readonly JsonSerializerSettings JsonSerializerSettings = new()
    {
        NullValueHandling = NullValueHandling.Ignore
    };

    private readonly IContentService _contentService;
    private readonly IChangesHistoryRepository _changesHistoryRepository;
    private readonly IEpisodeRepository _episodeRepository;
    private readonly IEpisodeService _episodeService;
    private readonly ITranslationRepository _translationRepository;

    public ChangesHistoryService(
        ITranslationRepository translationRepository,
        IContentService contentService,
        IChangesHistoryRepository changesHistoryRepository,
        IEpisodeRepository episodeRepository,
        IEpisodeService episodeService)
    {
        _translationRepository = translationRepository;
        _contentService = contentService;
        _changesHistoryRepository = changesHistoryRepository;
        _episodeRepository = episodeRepository;
        _episodeService = episodeService;
    }

    public async Task ApproveAsync(long historyId, long userId, CancellationToken token)
    {
        var changeHistoryUnits = await QueryAsync(new ChangesHistoryQueryModel()
        {
            Ids = new[] { historyId }
        }, token: token);
        var changeUnit = changeHistoryUnits.FirstOrDefault();

        if (changeUnit is { ApprovedAt: not null, ApprovedBy: not null })
        {
            throw new ChangesAlreadyApprovedException(approvedBy: changeUnit.ApprovedBy.Value);
        }

        if (changeUnit is null)
            throw new NullReferenceException(nameof(changeUnit));
        if (false) //TODO: changeUnit.CreatedBy == userId)
            throw new ArgumentException($"User - {userId} can't self-approve changes - {historyId}.");

        await using var transaction = await _changesHistoryRepository.BeginTransaction(token);
        await _changesHistoryRepository.ApproveAsync(historyId, userId, approvedAt: DateTimeOffset.UtcNow, token);

        switch (changeUnit.HistoryType)
        {
            case HistoryType.Content:
                await ApproveContentAsync(changeUnit, token);

                break;
            case HistoryType.Episode:
                await ApproveEpisodeAsync(changeUnit, token);
                break;
        }

        await transaction.CommitAsync(token);
    }

    private async Task ApproveEpisodeAsync(HistoryUnit changeUnit, CancellationToken token)
    {
        var changes = (HistoryUnit.JsonEpisodeChanges)changeUnit.Changes;
        if (changes.ContentId is null)
        {
            throw new EmptyRequiredPropertiesException(nameof(changes.ContentId));
        }

        var episodeId = changes.EpisodeId;
        if (changes.EpisodeId is null)
        {
            var episodeQuery = new QueryEpisode()
            {
                Numbers = new long[] { changes.Number },
                ContentIds = new long[] { changes.ContentId.Value },
            };

            var episodes = await _episodeRepository.QueryAsync(episodeQuery, token);
            if (episodes.Any())
            {
                episodeId = episodes.Single().Id;
            }
            else
            {
                episodeId = await _episodeRepository.InsertAsync(
                    Episode.CreateInsertModel(
                        changes.ContentId.Value,
                        changes.Number,
                        changes.Title,
                        changes.Image,
                        changeUnit.CreatedBy),
                    token);
            }
        }
        else
        {
            var episodeQuery = new QueryEpisode()
            {
                EpisodeIds = new long[] { changes.EpisodeId.Value },
                ContentIds = new long[] { changes.ContentId.Value },
            };

            var episodes = await _episodeRepository.QueryAsync(episodeQuery, token);
            if (!episodes.Any())
            {
                throw new EpisodeNotFoundException(changes.EpisodeId.Value);
            }

            var episode = episodes.Single();

            var updateEpisode = new Episode()
            {
                Id = episode.Id,
                ContentId = episode.ContentId,
                Number = changes.Number,
                Title = changes.Title ?? episode.Title,
                Image = changes.Image ?? episode.Image,
            };
            await _episodeRepository.UpdateAsync(updateEpisode, token);
        }

        if (changes.ContentId is null ||
            changes.TranslatorId is null ||
            changes.VideoScript is null)
        {
            throw new EmptyRequiredPropertiesException();
        }

        var translate = new DAL.Models.InsertTranslation(
            ContentId: changes.ContentId.Value,
            EpisodeId: episodeId.Value,
            TranslatorId: changes.TranslatorId ?? 0,
            Link: changes.VideoScript,
            CreatedAt: changeUnit.CreatedAt,
            CreatedBy: changeUnit.CreatedBy,
            Lang: changes.Language ?? Language.Unspecified,
            TranslationType: changes.TranslationType ?? TranslationType.Unspecified,
            Quality: changes.Quality ?? 0);

        await _translationRepository.InsertAsync(translate, token);
    }

    private async Task ApproveContentAsync(HistoryUnit changeUnit, CancellationToken token)
    {
        var contentChanges = (HistoryUnit.JsonContentChanges)changeUnit.Changes;
        if (changeUnit.TargetId is 0 or null)
        {
            // Для создания контента используется идентификатор из changes_history записи
            await _contentService.InsertAsync(new InsertContentModel(
                Id: changeUnit.Id,
                ImageId: contentChanges.ImageId ?? throw new ArgumentException(),
                Title: contentChanges.Title ?? throw new ArgumentException(),
                Description: contentChanges.Description ?? throw new ArgumentException(),
                ContentType: contentChanges.ContentType ?? throw new ArgumentException(),
                Country: contentChanges.Country ?? throw new ArgumentException(),
                Status: contentChanges.Status,
                Channel: contentChanges.Channel,
                EngTitle: contentChanges.EngTitle,
                OriginTitle: contentChanges.OriginTitle,
                Duration: contentChanges.Duration,
                ReleasedAt: contentChanges.ReleasedAt,
                PlannedSeries: contentChanges.PlannedSeries,
                MinAge: contentChanges.MinAge
            ), token);
        }
        else
        {
            var contents = await _contentService.GetByQueryAsync(
                new SearchFilters()
                {
                    ContentIds = new[] { changeUnit.TargetId.Value },
                },
                ContentSelectedInfo.None,
                token);

            if (!contents.Any())
            {
                throw new ContentNotFoundException(changeUnit.TargetId.Value);
            }

            var selectedContend = contents.Single();
            await _contentService.UpdateAsync(new UpdateContentModel(
                changeUnit.TargetId.Value,
                contentChanges.ImageId ?? selectedContend.ImageId,
                contentChanges.Title ?? selectedContend.Title,
                contentChanges.EngTitle ?? selectedContend.EngTitle,
                contentChanges.OriginTitle ?? selectedContend.OriginTitle,
                contentChanges.Description ?? selectedContend.Description,
                (int?)contentChanges.Country ?? (int)selectedContend.Country,
                (int?)contentChanges.ContentType ?? (int)selectedContend.Type,
                contentChanges.Duration ?? selectedContend.Duration,
                contentChanges.ReleasedAt ?? selectedContend.ReleasedAt,
                contentChanges.PlannedSeries ?? selectedContend.PlannedSeries,
                contentChanges.MinAge ?? selectedContend.MinAgeLimit
            ), token);

            if (contentChanges.Genres is not null)
            {
                //TODO: Create Genres
            }
        }
    }

    public async Task<HistoryUnit[]> QueryAsync(ChangesHistoryQueryModel query, CancellationToken token)
    {
        var changes = await _changesHistoryRepository.QueryAsync(new ChangesHistoryQuery
        {
            Ids = query.Ids,
            TargetIds = query.TargetIds,
            HistoryTypes = query.HistoryTypes,
            CreatedByIds = query.CreatedByIds,
            Limit = query.Limit,
            Offset = query.Offset,
        }, token);

        return changes.Select(historyUnit =>
        {
            var changesModel = historyUnit.DeserializeToJsonChanges();

            return new HistoryUnit
            {
                Id = historyUnit.Id,
                TargetId = historyUnit.TargetId,
                HistoryType = historyUnit.HistoryType,
                Changes = changesModel,
                CreatedBy = historyUnit.CreatedBy,
                CreatedAt = historyUnit.CreatedAt,
                ApprovedBy = historyUnit.ApprovedBy,
                ApprovedAt = historyUnit.ApprovedAt
            };
        }).ToArray();
    }

    public async Task<IReadOnlyCollection<HistoryChangesComparisons>> GetChangesComparisons(
        ChangesHistoryQueryModel query,
        CancellationToken token)
    {
        var changes = await QueryAsync(query, token);
        var episodesMap = await GetEpisodesMap(changes, token);
        var contentsMap = await GetContentMap(changes, token);

        return changes.Select(unit => unit.HistoryType switch
        {
            HistoryType.Content => unit.MapContentUnitToChangesComparison(contentsMap),
            HistoryType.Episode => unit.MapEpisodeUnitToChangesComparison(episodesMap),
            _ => throw new ArgumentOutOfRangeException(nameof(unit.HistoryType))
        }).ToArray();
    }

    public async Task<long> InsertChangesAsync(HistoryUnit historyUnit, CancellationToken token)
    {
        var jsonContentChanges = JsonConvert.SerializeObject(historyUnit.Changes, JsonSerializerSettings);

        var id = await _changesHistoryRepository.InsertAsync(new DAL.Models.HistoryUnit
        {
            TargetId = historyUnit.TargetId,
            HistoryType = historyUnit.HistoryType,
            Text = jsonContentChanges,
            CreatedBy = historyUnit.CreatedBy,
            CreatedAt = historyUnit.CreatedAt,
            ApprovedBy = historyUnit.ApprovedBy,
            ApprovedAt = historyUnit.ApprovedAt
        }, token);

        return id;
    }

    public async Task UpdateImageAsync(long historyId, long imageId, CancellationToken token)
    {
        var histories = await _changesHistoryRepository.QueryAsync(
            new ChangesHistoryQuery
            {
                Ids = new[] { historyId },
                HistoryTypes = new[] { HistoryType.Content }
            }, token);

        var history = histories.FirstOrDefault();
        if (history is null)
        {
            return;
        }

        var changes = JsonConvert.DeserializeObject<HistoryUnit.JsonContentChanges>(
            history.Text,
            JsonSerializerSettings
        );

        changes.ImageId = imageId;

        var jsonContentChanges = JsonConvert.SerializeObject(changes, JsonSerializerSettings);
        await _changesHistoryRepository.UpdateTextAsync(historyId, jsonContentChanges, token);
    }

    private async Task<Dictionary<long, Content>> GetContentMap(HistoryUnit[] changes, CancellationToken token)
    {
        var contentIds = changes
            .Where(unit => unit is { TargetId: > 0 })
            .Select(unit => unit.TargetId!.Value)
            .ToArray();

        var contents = Array.Empty<Content>();
        if (contentIds.Any())
        {
            contents = await _contentService.QueryAsync(new ContentQuery()
            {
                Ids = contentIds
            }, token);
        }

        return contents.ToDictionary(unit => unit.Id);
    }

    private async Task<Dictionary<long, Models.Episode.Episode>> GetEpisodesMap(
        HistoryUnit[] historyUnits, 
        CancellationToken token)
    {
        var episodeIds = historyUnits
            .Where(x => x is { TargetId: > 0, HistoryType: HistoryType.Episode })
            .Select(x => x.TargetId!.Value)
            .ToArray();
    
        var episodes = Array.Empty<Models.Episode.Episode>();
        if (episodeIds.Any())
        {
            episodes = await _episodeService.QueryAsync(new EpisodeQuery()
            {
                EpisodeIds = episodeIds,
            }, token);
        }

        return episodes.ToDictionary(x => x.Id);
    }
}

