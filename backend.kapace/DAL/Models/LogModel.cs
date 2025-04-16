namespace backend.kapace.DAL.Models;

public record LogModel
{
    public long UserId { get; init; }
    public Value[] Changes { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public string[] Metadata { get; set; }

    public record Value(string OldValue, string NewValue);
}