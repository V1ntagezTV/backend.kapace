namespace backend.kapace.DAL.Models;

public class QueryContent
{
    public long[]? Ids { get; init; }
    public int[]? Countries { get; init; }
    public int[]? Statuses { get; init; }
    public int[]? Types { get; init; }
    public int Limit { get; init; }
    public int Offset { get; init; }

}