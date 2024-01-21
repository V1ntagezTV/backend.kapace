namespace backend.kapace.DAL.Experimental;

public abstract class BaseQuery
{
    private readonly Dictionary<string, object?> _whereFilters = new();
    private readonly List<string> _orderBy = new();

    internal IReadOnlyDictionary<string, object?> WhereFilters => _whereFilters;
    internal IReadOnlyCollection<string> OrderBy => _orderBy;
}