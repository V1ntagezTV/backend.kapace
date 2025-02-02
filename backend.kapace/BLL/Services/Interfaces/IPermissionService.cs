using backend.kapace.BLL.Models.Permissions;

namespace backend.kapace.BLL.Services.Interfaces;

public interface IPermissionService
{
    Task<Permission[]> Query(PermissionQuery query, CancellationToken token);

    Task Insert(Permission permission, CancellationToken token);
}