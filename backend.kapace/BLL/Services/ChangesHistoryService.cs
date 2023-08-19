using backend.kapace.BLL.Enums;
using backend.kapace.BLL.Models;
using backend.kapace.BLL.Services.Interfaces;
using backend.kapace.BLL.Validators;
using backend.kapace.DAL.Models;
using backend.kapace.DAL.Repository.Interfaces;
using backend.Models.Enums;
using Newtonsoft.Json;
using HistoryUnit = backend.kapace.BLL.Services.Interfaces.HistoryUnit;

namespace backend.kapace.BLL.Services;

internal class ChangesHistoryService : IChangesHistoryService
{
    private static JsonSerializerSettings _jsonSerializerSettings = new JsonSerializerSettings
    {
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
        if (changeUnit.CreatedBy == userId)
            throw new ArgumentException($"User - {userId} can't self-approve changes - {historyId}.");

        await _changesHistoryService.ApproveAsync(historyId, userId, approvedAt: DateTimeOffset.Now, token);

        switch (changeUnit.HistoryType)
        {
            case HistoryType.Content:
                var contentChanges = (HistoryUnit.JsonContentChanges)changeUnit.Changes;
                if (changeUnit.TargetId is null)
                {
                    ContentValidator.ValidateContentRequiredProperties(contentChanges);
                    await _contentService.InsertAsync(new InsertContentModel(
                            Image: contentChanges.Image ?? throw new ArgumentException(""),
                            Title: contentChanges.Title,
                            Description: contentChanges.Description,
                            ContentType: (ContentType)contentChanges.ContentType,
                            Country: (Country)contentChanges.Country,
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

    public async Task InsertChangesAsync(HistoryUnit historyUnit, CancellationToken token)
    {
        var jsonContentChanges = JsonConvert.SerializeObject(historyUnit.Changes, _jsonSerializerSettings);

        await _changesHistoryService.InsertAsync(new DAL.Models.HistoryUnit
        {
            TargetId = historyUnit.TargetId,
            HistoryType = historyUnit.HistoryType,
            Text = jsonContentChanges,
            CreatedBy = historyUnit.CreatedBy,
            CreatedAt = historyUnit.CreatedAt,
            ApprovedBy = historyUnit.ApprovedBy,
            ApprovedAt = historyUnit.ApprovedAt
        }, token);
    }
}