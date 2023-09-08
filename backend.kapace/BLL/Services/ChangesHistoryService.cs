using backend.kapace.BLL.Enums;
using backend.kapace.BLL.Models;
using backend.kapace.BLL.Services.Interfaces;
using backend.kapace.DAL.Models;
using backend.kapace.DAL.Repository.Interfaces;
using Newtonsoft.Json;
using HistoryUnit = backend.kapace.BLL.Services.Interfaces.HistoryUnit;

namespace backend.kapace.BLL.Services;

internal class ChangesHistoryService : IChangesHistoryService
{
    private static readonly JsonSerializerSettings JsonSerializerSettings = new() {
        NullValueHandling = NullValueHandling.Ignore
    };
    
    private readonly IContentService _contentService;
    private readonly IChangesHistoryRepository _changesHistoryService;

    public ChangesHistoryService(
        IContentService contentService,
        IChangesHistoryRepository changesHistoryService)
    {
        _contentService = contentService;
        _changesHistoryService = changesHistoryService;
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
                var contentChanges = (HistoryUnit.JsonContentChanges)changeUnit.Changes;
                if (changeUnit.TargetId is 0 or null)
                {
                    await _contentService.InsertAsync(new InsertContentModel(
                            // Для создания контента используется идентификатор из changes_history записи.
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
                        ),
                        token);
                }
                else
                { /***
                    await _contentService.UpdateAsync(new CreateContentModel(
                        Id: changeUnit.Id,
                        Image: contentChanges.Image,
                        Title: contentChanges.Title,
                        EngTitle: contentChanges.EngTitle,
                        OriginalTitle: contentChanges.OriginalTitle,
                        Description: contentChanges.Description,
                        Country: (int?)contentChanges.Country,
                        ContentType: (int?)contentChanges.ContentType,
                        Genres: contentChanges.Genres,
                        Duration: contentChanges.Duration,
                        ReleasedAt: contentChanges.ReleasedAt,
                        PlannedSeries: contentChanges.PlannedSeries,
                        MinAge: contentChanges.MinAge
                    ), token);
                    ***/
                }

                break;
            case HistoryType.Episode:
                // TODO: episode edit
                //var episodeChanges = JsonConvert.DeserializeObject<JsonEpisodeChanges>(changeUnit.Text);
                break;
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
                ? JsonConvert.DeserializeObject<HistoryUnit.JsonContentChanges>(x.Text) ?? new HistoryUnit.JsonChanges()
                : JsonConvert.DeserializeObject<HistoryUnit.JsonEpisodeChanges>(x.Text) ?? new HistoryUnit.JsonChanges(),
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
            },
            token
        );

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