namespace backend.kapace.BLL.Services;

public record TranslatorQuery(
    long[]? TranslatorIds,
    string? Search,
    int? Offset,
    int? Limit
) {
    public static explicit operator DAL.Models.Query.TranslatorQuery(TranslatorQuery query) {
        return new DAL.Models.Query.TranslatorQuery(
            query.TranslatorIds,
            query.Search,
            query.Offset,
            query.Limit);
    }
};