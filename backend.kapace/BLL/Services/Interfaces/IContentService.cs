using backend.kapace.BLL.Enums;
using backend.kapace.BLL.Models;
using backend.kapace.BLL.Models.VideoService;
using backend.kapace.Models;

namespace backend.kapace.BLL.Services.Interfaces;

public interface IContentService
{
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
    
    Task<SearchContentUnit[]> SearchBy(string? search, CancellationToken token);
    
    Task IncrementViews(long contentId, CancellationToken token);
}