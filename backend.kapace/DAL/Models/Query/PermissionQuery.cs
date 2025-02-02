namespace backend.kapace.DAL.Models.Query;

public class PermissionQuery
{
    public long[] Ids { get; init; }
    public string[] Aliases { get; init; }
    public long[] CreatedBy { get; init; }
}