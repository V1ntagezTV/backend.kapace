using backend.kapace.BLL.Enums;
using backend.kapace.BLL.Exceptions;
using backend.kapace.BLL.Models;
using backend.kapace.BLL.Services.Interfaces;
using backend.kapace.DAL.Models;
using backend.kapace.DAL.Repository.Interfaces;
using Newtonsoft.Json;
using HistoryUnit = backend.kapace.BLL.Services.Interfaces.HistoryUnit;

namespace backend.kapace.BLL.Services;

public class ChangesHistoryService : IChangesHistoryService
{
    private static readonly JsonSerializerSettings JsonSerializerSettings = new() {
        NullValueHandling = NullValueHandling.Ignore
    };
    
    private readonly IContentService _contentService;
    private readonly IChangesHistoryRepository _changesHistoryService;
    private readonly IEpisodeService _episodeService;

    public ChangesHistoryService(
        IContentService contentService,
        IChangesHistoryRepository changesHistoryService,
        IEpisodeService episodeService)
    {
        _contentService = contentService;
        _changesHistoryService = changesHistoryService;
        _episodeService = episodeService;
    }

    public async Task ApproveAsync(long historyId, long userId, CancellationToken token)
    {
        var changeHistoryUnits = await QueryAsync(new[] { historyId }, token);
        var changeUnit = changeHistoryUnits.FirstOrDefault();

        if (changeUnit is null)
            throw new NullReferenceException(nameof(changeUnit));
        if (false) //TODO: changeUnit.CreatedBy == userId)
            throw new ArgumentException($"User - {userId} can't self-approve changes - {historyId}.");

        await _changesHistoryService.ApproveAsync(historyId, userId, approvedAt: DateTimeOffset.UtcNow, token);

        switch (changeUnit.HistoryType)
        {
            case HistoryType.Content:
                await ApproveContentAsync(changeUnit, token);

                break;
            case HistoryType.Episode:
                var episodeChanges = (HistoryUnit.JsonEpisodeChanges)changeUnit.Changes;
                if (episodeChanges.EpisodeId is null or 0)
                {
                    // TODO: Не хватает ContentId
                    // await _episodeService.InsertAsync(Episode.CreateInsertModel(episodeChanges), token);
                }
                else
                {
                    //await _episodeService.UpdateAsync(token);
                }
                break;
        }
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
                    OriginTitle: contentChanges.OriginalTitle,
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
                contentChanges.OriginalTitle ?? selectedContend.OriginTitle,
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

    public async Task<HistoryUnit[]> QueryAsync(long[] ids, CancellationToken token)
    {
        var changes = await _changesHistoryService.QueryAsync(new ChangesHistoryQuery()
        {
            Ids = ids,
        }, token);

        return changes.Select(x => new HistoryUnit
        {
            Id = x.Id,
            TargetId = x.TargetId,
            HistoryType = x.HistoryType,
            Changes = x.HistoryType == HistoryType.Content 
                ? JsonConvert.DeserializeObject<HistoryUnit.JsonContentChanges>(x.Text) 
                ?? new HistoryUnit.JsonChanges()
                : JsonConvert.DeserializeObject<HistoryUnit.JsonEpisodeChanges>(x.Text) 
                ?? new HistoryUnit.JsonChanges(),
            CreatedBy = x.CreatedBy,
            CreatedAt = x.CreatedAt,
            ApprovedBy = x.ApprovedBy,
            ApprovedAt = x.ApprovedAt
        }).ToArray();
    }

    public async Task<long> InsertChangesAsync(HistoryUnit historyUnit, CancellationToken token)
    {
        var jsonContentChanges = JsonConvert.SerializeObject(historyUnit.Changes, JsonSerializerSettings);

        var id = await _changesHistoryService.InsertAsync(new DAL.Models.HistoryUnit
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
        var histories = await _changesHistoryService.QueryAsync(
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
        await _changesHistoryService.UpdateTextAsync(historyId, jsonContentChanges, token);
    }
}