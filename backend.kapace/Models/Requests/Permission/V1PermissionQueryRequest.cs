namespace backend.kapace.Models.Requests.Permission;

public class V1PermissionQueryRequest
{
    public long[] PermissionIds { get; init; }
    public long[] CreatedBy { get; init; }
    public string[] Aliases { get; init; }
}