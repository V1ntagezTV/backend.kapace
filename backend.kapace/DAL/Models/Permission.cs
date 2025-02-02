namespace backend.kapace.DAL.Models;

public class Permission
{
    public long Id { get; init; }
    public string Alias { get; init; }
    public string Description { get; init; }
    public long CreatedBy { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
}