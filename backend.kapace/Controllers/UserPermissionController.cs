using backend.kapace.DAL.Models;
using backend.kapace.DAL.Models.Query;
using backend.kapace.DAL.Repository.Interfaces;
using backend.kapace.Models.Requests.UserPermission;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.kapace.Controllers;

// TODO: добавить валидаторы

[ApiController]
[Authorize(Roles = "admin")]
[Route("v1/user-permissions")]
public class UserPermissionController : Controller
{
    private readonly IUserPermissionRepository _repository;

    public UserPermissionController(IUserPermissionRepository repository)
    {
        _repository = repository;
    }
    
    [HttpPost("query")]
    public async Task<ActionResult> V1Query(V1UserPermissionQueryRequest request, CancellationToken token)
    {
        var users = await _repository.Query(new UserPermissionsQuery
        {
            UserIds = request.UserIds,
            RoleIds = request.RoleIds
        }, token);

        var units = users
            .Select(userPermission => new
            {
                UserId = userPermission.UserId,
                PermissionId = userPermission.PermissionId,
                CreatedBy = userPermission.GivedBy,
                CreatedAt = userPermission.CreatedAt
            })
            .ToArray();

        return Ok(new
        {
            Roles = units
        });
    }
    
    [HttpPost("create")]
    public async Task<ActionResult> Create(V1UserRoleCreateRequest request, CancellationToken token)
    {
        var initSql = new UserPermission
        {
            UserId = request.UserId,
            PermissionId = request.PermissionId,
            CreatedAt = DateTimeOffset.UtcNow,
            GivedBy = request.GivedBy
        };

        await _repository.Insert(initSql, token);

        return Ok();
    }
}