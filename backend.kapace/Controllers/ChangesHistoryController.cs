using backend.kapace.BLL.Enums;
using backend.kapace.BLL.Models;
using backend.kapace.BLL.Services.Interfaces;
using backend.kapace.Models.Requests;
using backend.kapace.Models.Response;
using Microsoft.AspNetCore.Mvc;
using HistoryUnit = backend.kapace.BLL.Services.Interfaces.HistoryUnit;

namespace backend.kapace.Controllers;

[ApiController]
[Route("v1/changes")]
public class ChangesHistoryController : Controller
{
    private readonly IChangesHistoryService _changesHistoryService;

    public ChangesHistoryController(IChangesHistoryService changesHistoryService)
    {
        _changesHistoryService = changesHistoryService;
    }

    [HttpPost("create-content")]
    public async Task<ActionResult<V1CreateContentResponse>> V1CreateContentHistory(
        V1CreateContentHistoryRequest historyRequest,
        CancellationToken token)
    {
        var newHistoryUnit = new HistoryUnit
        {
            TargetId = historyRequest.ContentId,
            HistoryType = HistoryType.Content,
            Changes = historyRequest.ChangeableFields is not null ? new HistoryUnit.JsonContentChanges
            {
                ImageId = historyRequest.ChangeableFields.ImageId,
                Title = historyRequest.ChangeableFields.Title,
                EngTitle = historyRequest.ChangeableFields.EngTitle,
                OriginTitle = historyRequest.ChangeableFields.OriginalTitle,
                Description = historyRequest.ChangeableFields.Description,
                Country = historyRequest.ChangeableFields.Country,
                ContentType = historyRequest.ChangeableFields.ContentType,
                Genres = historyRequest.ChangeableFields.Genres,
                Duration = historyRequest.ChangeableFields.Duration,
                ReleasedAt = historyRequest.ChangeableFields.ReleasedAt,
                PlannedSeries = historyRequest.ChangeableFields.PlannedSeries,
                MinAge = historyRequest.ChangeableFields.MinAge
            } : new HistoryUnit.JsonContentChanges(),
            CreatedBy = historyRequest.CreatedBy,
            CreatedAt = DateTimeOffset.Now,
            ApprovedBy = null,
            ApprovedAt = null,
        };

        var insertedId = await _changesHistoryService.InsertChangesAsync(newHistoryUnit, token);

        return new V1CreateContentResponse(insertedId);
    }

    [HttpPost("create-episode")]
    public async Task<ActionResult<V1CreateEpisodeHistoryResponse>> V1CreateEpisodeHistory(
        V1CreateEpisodeHistoryRequest historyRequest,
        CancellationToken token) 
    {
        var newHistoryUnit = new HistoryUnit
        {
            TargetId = historyRequest.ContentId,
            HistoryType = HistoryType.Episode,
            Changes = historyRequest.ChangeableFields is not null 
                ? new HistoryUnit.JsonEpisodeChanges()
                    {
                        ContentId = historyRequest.ContentId,
                        EpisodeId = historyRequest.ChangeableFields.EpisodeId,
                        TranslatorId = historyRequest.ChangeableFields.TranslatorId,
                        Number = historyRequest.ChangeableFields.Number,
                        Title = historyRequest.ChangeableFields.Title,
                        Image = historyRequest.ChangeableFields.Image,
                        VideoScript = historyRequest.ChangeableFields.VideoScript,
                        TranslationType = historyRequest.ChangeableFields.TranslationType,
                        Language = historyRequest.ChangeableFields.Language,
                        Quality = historyRequest.ChangeableFields.Quality,
                    }
                : new HistoryUnit.JsonEpisodeChanges(),
            CreatedBy = historyRequest.CreatedBy,
            CreatedAt = DateTimeOffset.Now,
            ApprovedBy = null,
            ApprovedAt = null,
        };

        var insertedId = await _changesHistoryService.InsertChangesAsync(newHistoryUnit, token);

        return new V1CreateEpisodeHistoryResponse(insertedId);
    }

    [HttpPost("approve")]
    public async Task V1Approve(V1ApproveRequest request, CancellationToken token)
    {
        await _changesHistoryService.ApproveAsync(request.HistoryId, request.UserId, token);
    }

    [HttpPost("get-list")]
    public async Task<ActionResult> V1GetChanges(V1GetChangesRequest request, CancellationToken token)
    {
        var query = new ChangesHistoryQueryModel
        {
            Ids = request.Ids,
            TargetIds = request.TargetIds,
            CreatedByIds = request.CreatedByIds,
            HistoryTypes = request.HistoryTypes,
            Limit = request.Limit,
            Offset = request.Offset
        };

        var result = await _changesHistoryService.GetList(query, token);

        return Ok(new
        {
            Changes = result
                .Select(x => new
                {
                    HistoryId = x.HistoryId,
                    TargetId = x.TargetId,
                    Title = x.Title,
                    HistoryType = x.HistoryType,
                    Text = x.Text,
                    CreatedBy = x.CreatedBy,
                    CreatedAt = x.CreatedAt,
                    ApprovedBy = x.ApprovedBy,
                    ApprovedAt = x.ApprovedAt,
                })
                .ToArray()
        });
    }
}