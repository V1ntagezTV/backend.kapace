using backend.kapace.DAL.Models;
using backend.kapace.Models;

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
}