namespace backend.kapace.Models.Requests.Genre;

public record V1GenreQueryRequest(
    string? Search,
    long[]? GenreIds,
    string[]? Names,
    int? Limit,
    int? Offset);