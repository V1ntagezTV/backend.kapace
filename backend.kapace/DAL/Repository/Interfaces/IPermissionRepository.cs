using backend.kapace.DAL.Models;
using backend.kapace.DAL.Models.Query;

namespace backend.kapace.DAL.Repository.Interfaces;

public interface IPermissionRepository
{
    Task<IReadOnlyCollection<Permission>> Query(PermissionQuery query, CancellationToken token);
    Task Insert(Permission permission, CancellationToken token);
}