namespace backend.kapace.DAL.Models.Query;


public record TranslatorQuery(
    long[]? TranslatorIds,
    string? Search,
    int? Offset,
    int? Limit
);