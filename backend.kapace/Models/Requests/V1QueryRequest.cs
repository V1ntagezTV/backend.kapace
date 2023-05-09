namespace backend.kapace.Models.Requests;

public class V1QueryRequest
{
    public long[] Ids { get; init; } = Array.Empty<long>();
    
    public QueryPaging QueryPaging { get; init; } = new QueryPaging(20, 0);
}