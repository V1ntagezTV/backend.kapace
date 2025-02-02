namespace backend.kapace.Models.Requests;

public record V1RequestGetSearchByRequest(
    string? Search,
    int Limit = 10);