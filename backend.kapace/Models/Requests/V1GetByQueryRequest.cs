using backend.kapace.BLL.Enums;
using backend.Models.Enums;

namespace backend.kapace.Models.Requests;

public class V1GetByQueryRequest
{
    public string Search { get; set; } = ""; // Can be сontent ru-name, eng-name
    public V1GetByQuerySearchFilters Filters { get; set; } = new();
    public QueryPaging QueryPaging { get; set; } = new(20, 0);
    public ContentSelectedInfo SelectedInfo { get; set; } = ContentSelectedInfo.None;

    public class V1GetByQuerySearchFilters
    {
        public Country[] Countries { get; set; } = Array.Empty<Country>();
        public ContentType[] ContentTypes { get; set; } = Array.Empty<ContentType>();
        public ContentStatus[] ContentStatuses { get; set; } = Array.Empty<ContentStatus>();
        public long[] GenreIds { get; set; } = Array.Empty<long>();
    }
}