using backend.kapace.BLL.Enums;
using backend.kapace.BLL.Models;
using backend.kapace.BLL.Models.VideoService;
using backend.kapace.Models;

namespace backend.kapace.BLL.Services.Interfaces;

public interface IContentService
{
    [Obsolete("Используйте GetByQueryAsync")]
    Task<Content[]> QueryAsync(ContentQuery query, CancellationToken token);

    Task<IReadOnlyDictionary<MainPageType, Content[]>> GetOrderByMapsAsync(
        IReadOnlyCollection<MainPageType> pageTypes,
        QueryPaging pagingSettings,
        CancellationToken token);

    Task<FullContent> GetFullAsync(
        long contentId, 
        long? userId, 
        ContentSelectedInfo selectedInfo,
        CancellationToken token);

    Task<IReadOnlyCollection<GetByQueryResult>> GetByQueryAsync(
        string search, 
        SearchFilters searchFilters, 
        QueryPaging? queryPaging,
        ContentSelectedInfo selectedInfo,
        CancellationToken token);

    Task<IReadOnlyCollection<GetByQueryResult>> GetByQueryAsync(
        SearchFilters searchFilters,
        ContentSelectedInfo selectedInfo,
        CancellationToken token);

    Task<long> InsertAsync(InsertContentModel model, CancellationToken token);
    
    Task UpdateAsync(UpdateContentModel newContent, CancellationToken token);
}

public record UpdateContentModel(
    long ContentId,
    long? ImageId, 
    string? Title, 
    string? EngTitle, 
    string? OriginalTitle, 
    string? Description, 
    int? Country,
    int? ContentType, 
    int? Duration, 
    DateTimeOffset? ReleasedAt,
    int? PlannedSeries,
    int? MinAge);