namespace backend.kapace.DAL.Models;

public class UserPermission
{
    public long UserId { get; init; }
    public long PermissionId { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public long GivedBy { get; init; }
}