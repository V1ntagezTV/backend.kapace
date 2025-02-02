using backend.kapace.BLL.Models.Permissions;
using backend.kapace.BLL.Services.Interfaces;
using backend.kapace.Models.Requests.Permission;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.kapace.Controllers;

[ApiController]
[Authorize("admin")]
[Route("v1/permission")]
public class PermissionController : Controller
{
    private readonly IPermissionService _permissionService;

    public PermissionController(IPermissionService permissionService)
    {
        _permissionService = permissionService;
    }

    [HttpPost("query")]
    public async Task<ActionResult> V1Query(V1PermissionQueryRequest request, CancellationToken token)
    {
        var permissions = await _permissionService.Query(new()
        {
            PermissionIds = request.PermissionIds,
            Aliases = request.Aliases,
            CreatedBy = request.CreatedBy
        }, token);

        var units = permissions
            .Select(p => new
            {
                p.Id,
                p.Alias,
                p.Description,
                p.CreatedBy,
                p.CreatedAt
            })
            .ToArray();

        return Ok(new
        {
            Roles = units
        });
    }

    [HttpPost("create")]
    public async Task<ActionResult> Create(V1PermissionCreateRequest request, CancellationToken token)
    {
        var model = new Permission(
            0,
            request.Alias,
            request.Description,
            request.CreatedBy,
            DateTimeOffset.UtcNow);

        await _permissionService.Insert(model, token);

        return Ok();
    }
}