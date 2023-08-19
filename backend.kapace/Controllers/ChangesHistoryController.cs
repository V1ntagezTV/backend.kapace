using backend.kapace.BLL.Enums;
using backend.kapace.BLL.Services.Interfaces;
using backend.kapace.Models.Requests;
using Microsoft.AspNetCore.Mvc;

namespace backend.kapace.Controllers;

[Route("v1/changes")]
public class ChangesHistoryController : Controller
{
    private readonly IChangesHistoryService _changesHistoryService;

    internal ChangesHistoryController(IChangesHistoryService changesHistoryService)
    {
        _changesHistoryService = changesHistoryService;
    }

    [HttpPost("upsert-content")]
    public async Task<ActionResult> V1ContentUpsert(V1UpsertContentRequest request, CancellationToken token)
    {
        var newHistoryUnit = new HistoryUnit
        {
            TargetId = request.ContentId,
            HistoryType = HistoryType.Content,
            Changes = request.ChangeableFields is not null ? new HistoryUnit.JsonContentChanges
            {
                Image = request.ChangeableFields.Image,
                Title = request.ChangeableFields.Title,
                EngTitle = request.ChangeableFields.EngTitle,
                OriginalTitle = request.ChangeableFields.OriginalTitle,
                Description = request.ChangeableFields.Description,
                Country = request.ChangeableFields.Country,
                ContentType = request.ChangeableFields.ContentType,
                Genres = request.ChangeableFields.Genres,
                Duration = request.ChangeableFields.Duration,
                ReleasedAt = request.ChangeableFields.ReleasedAt,
                PlannedSeries = request.ChangeableFields.PlannedSeries,
                MinAge = request.ChangeableFields.MinAge
            } : new HistoryUnit.JsonContentChanges(),
            CreatedBy = request.CreatedBy,
            CreatedAt = DateTimeOffset.Now,
            ApprovedBy = null,
            ApprovedAt = null,
        };

        await _changesHistoryService.InsertChangesAsync(newHistoryUnit, token);

        return new OkResult();
    }

    [HttpPost("approve")]
    public async Task V1Approve(V1ApproveRequest request, CancellationToken token)
    {
        await _changesHistoryService.ApproveAsync(request.HistoryId, request.UserId, token);
    }
}