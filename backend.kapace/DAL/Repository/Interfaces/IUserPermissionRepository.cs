using backend.kapace.DAL.Models;
using backend.kapace.DAL.Models.Query;

namespace backend.kapace.DAL.Repository.Interfaces;

public interface IUserPermissionRepository
{
    Task<IReadOnlyCollection<UserPermission>> Query(UserPermissionsQuery query, CancellationToken token);
    Task Insert(UserPermission userPermission, CancellationToken token);
}