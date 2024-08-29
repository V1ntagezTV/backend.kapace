namespace backend.kapace.Models.Requests.Translator;

public record TranslatorQueryRequest(
    long[]? TranslatorIds,
    string? Search,
    int? Limit,
    int? Offset
);
