using backend.kapace.BLL.Models.Permissions;
using backend.kapace.BLL.Services.Interfaces;
using backend.kapace.DAL.Repository.Interfaces;

namespace backend.kapace.BLL.Services;

public class PermissionService : IPermissionService
{
    private readonly IPermissionRepository _permissionRepository;

    public PermissionService(IPermissionRepository permissionRepository)
    {
        _permissionRepository = permissionRepository;
    }

    public async Task<Permission[]> Query(PermissionQuery query, CancellationToken token)
    {
        var permissions = await _permissionRepository.Query(new DAL.Models.Query.PermissionQuery
        {
            Ids = query.PermissionIds,
            Aliases = query.Aliases,
            CreatedBy = query.CreatedBy
        }, token);

        return permissions
            .Select(p => new Permission(p.Id, p.Alias, p.Description, p.CreatedBy, p.CreatedAt))
            .ToArray();
    }

    public async Task Insert(Permission permission, CancellationToken token)
    {
        await _permissionRepository.Insert(new DAL.Models.Permission
        {
            Id = permission.Id,
            Alias = permission.Alias,
            Description = permission.Description,
            CreatedBy = permission.CreatedBy,
            CreatedAt = permission.CreatedAt
        }, token);
    }
}