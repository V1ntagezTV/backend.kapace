using backend.kapace.BLL.Enums;
using backend.kapace.BLL.Services.Interfaces;
using backend.kapace.Models.Requests;
using backend.kapace.Models.Response;
using Microsoft.AspNetCore.Mvc;

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
                OriginalTitle = historyRequest.ChangeableFields.OriginalTitle,
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

    [HttpPost("approve")]
    public async Task V1Approve(V1ApproveRequest request, CancellationToken token)
    {
        await _changesHistoryService.ApproveAsync(request.HistoryId, request.UserId, token);
    }
}