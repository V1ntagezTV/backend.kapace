﻿using backend.kapace.BLL.Enums;
using backend.kapace.BLL.Models;
using backend.kapace.BLL.Services.Interfaces;
using backend.kapace.Models.Requests;
using backend.kapace.Models.Response;
using Microsoft.AspNetCore.Mvc;
using HistoryUnit = backend.kapace.BLL.Models.HistoryChanges.HistoryUnit;

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
                    Channel = historyRequest.ChangeableFields.Channel,
                    Status = historyRequest.ChangeableFields.ContentStatus,
                    Country = historyRequest.ChangeableFields.Country,
                    ContentType = historyRequest.ChangeableFields.ContentType,
                    Genres = historyRequest.ChangeableFields.Genres,
                    Duration = historyRequest.ChangeableFields.Duration,
                    ReleasedAt = historyRequest.ChangeableFields.ReleasedAt,
                    PlannedSeries = historyRequest.ChangeableFields.PlannedSeries,
                    MinAge = historyRequest.ChangeableFields.MinAge,
                }
                : new HistoryUnit.JsonContentChanges(),
            CreatedBy = historyRequest.CreatedBy,
            CreatedAt = DateTimeOffset.Now,
            ApprovedBy = null,
            ApprovedAt = null,
        };

        var insertedId = await _changesHistoryService.InsertContentChangesAsync(newHistoryUnit, token);

        return Ok(new V1CreateContentResponse(insertedId));
    }

    [HttpPost("create-episode")]
    public async Task<ActionResult> V1CreateEpisodeHistory(
        V1CreateEpisodeHistoryRequest historyRequest,
        CancellationToken token) 
    {
        var newHistoryUnit = new HistoryUnit
        {
            TargetId = historyRequest.ContentId,
            HistoryType = HistoryType.Episode,
            Changes = new HistoryUnit.JsonEpisodeChanges()
            {
                ContentId = historyRequest.ContentId,
                EpisodeId = historyRequest.ChangeableFields.EpisodeId,
                TranslatorId = historyRequest.ChangeableFields.TranslatorId,
                Number = historyRequest.ChangeableFields.Number,
                Title = historyRequest.ChangeableFields.Title,
                ImageId = historyRequest.ChangeableFields.Image,
                VideoScript = historyRequest.ChangeableFields.VideoScript,
                TranslationType = historyRequest.ChangeableFields.TranslationType,
                Language = historyRequest.ChangeableFields.Language,
                Quality = historyRequest.ChangeableFields.Quality,
            },
            CreatedBy = historyRequest.CreatedBy,
            CreatedAt = DateTimeOffset.Now,
            ApprovedBy = null,
            ApprovedAt = null,
        };

        var insertedId = await _changesHistoryService.InsertChangesAsync(newHistoryUnit, token);

        return Ok(new V1CreateEpisodeHistoryResponse(insertedId));
    }

    [HttpPost("approve")]
    public async Task<ActionResult> V1Approve(V1ApproveRequest request, CancellationToken token)
    {
        await _changesHistoryService.ApproveAsync(request.HistoryId, request.UserId, token);

        return Ok();
    }

    [HttpPost("get-changes-comparisons")]
    public async Task<ActionResult> V1GetChangesComparisons(
        V1GetChangesComparisonsRequest comparisonsRequest, 
        CancellationToken token)
    {
        var query = new ChangesHistoryQueryModel
        {
            Ids = comparisonsRequest.Ids,
            TargetIds = comparisonsRequest.TargetIds,
            CreatedByIds = comparisonsRequest.CreatedByIds,
            HistoryTypes = comparisonsRequest.HistoryTypes,
            Approved = comparisonsRequest.Approved,
            OrderBy = comparisonsRequest.OrderBy,
            Limit = comparisonsRequest.Limit,
            Offset = comparisonsRequest.Offset
        };

        var result = await _changesHistoryService.GetChangesComparisons(query, token);

        return Ok(new
        {
            Changes = result.Select(unit => new
            {
                HistoryId = unit.HistoryId,
                TargetId = unit.TargetId,
                Title = unit.Title,
                HistoryType = unit.HistoryType,
                ImageId = unit.ImageId,
                FieldsComparisons = unit.FieldsComparisons
                    .Select(field => new
                    {
                        Name = field.Key,
                        OldValue = field.Value.oldValue,
                        NewValue = field.Value.newValue
                    }).ToArray(),
                CreatedBy = unit.CreatedBy,
                CreatedAt = unit.CreatedAt,
                ApprovedBy = unit.ApprovedBy,
                ApprovedAt = unit.ApprovedAt,
            }).ToArray()
        });
    }
}