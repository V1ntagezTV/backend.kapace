using backend.kapace.DAL.Models;
using backend.kapace.Models;
using QueryContent = backend.kapace.DAL.Models.QueryContent;

namespace backend.kapace.DAL.Repository.Interfaces;

public interface IContentRepository
{
    Task<Content[]> QueryAsync(QueryContent query, CancellationToken token);

    Task<IReadOnlyCollection<Content>> GetOrderedByPopularAsync(
        int forLastDaysCount, 
        QueryPaging paging,
        CancellationToken token);

    Task<IReadOnlyCollection<Content>> GetOrderedByNewestAsync(
        QueryPaging paging,
        CancellationToken token);

    Task<IReadOnlyCollection<Content>> GetOrderedByLastUpdatedAsync(
        QueryPaging paging,
        CancellationToken token);

    Task<long> InsertAsync(InsertContentQuery query, CancellationToken token);

    Task UpdateAsync(ContentUpdateQuery model, CancellationToken token);

    Task<IReadOnlyCollection<Content>> SearchByText(string? search, CancellationToken token);

    Task IncrementViews(long contentId, CancellationToken token);
    
    Task IncrementOutEpisodesCounter(long contentId, CancellationToken token);
}