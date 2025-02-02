namespace backend.kapace.Models.Requests.UserPermission;

public record V1UserPermissionQueryRequest(long[] UserIds, long[] RoleIds);