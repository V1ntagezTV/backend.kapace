namespace backend.kapace.BLL.Models.Genre;

public class GenreQuery
{
    public long[]? GenreIds {get; init; }
    public string[]? Names {get; init; }
    public int? Limit {get; init; }
    public int? Offset { get; init; }
    public string? Search { get; init; }
}