namespace backend.kapace.DAL.Models;

public record StaticFile
{
    public long Id { get; init; }
    public string FileName { get; init; }
    public int LinkType { get; init; }
    public long LinkId { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
}