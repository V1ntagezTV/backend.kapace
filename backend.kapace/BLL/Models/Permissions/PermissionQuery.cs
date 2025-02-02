namespace backend.kapace.BLL.Models.Permissions;

public class PermissionQuery
{
    public long[] PermissionIds { get; init; }
    public long[] CreatedBy { get; init; }
    public string[] Aliases { get; init; }
}