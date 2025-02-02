namespace backend.kapace.Models.Requests.UserPermission;

public record V1UserRoleCreateRequest(
    long UserId, 
    long PermissionId,
    long GivedBy);