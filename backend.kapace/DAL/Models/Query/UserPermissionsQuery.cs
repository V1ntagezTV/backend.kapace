namespace backend.kapace.DAL.Models.Query;

public class UserPermissionsQuery
{
    public long[] UserIds { get; init; }
    public long[] RoleIds { get; init; }
}