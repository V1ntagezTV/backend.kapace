using backend.kapace.BLL.Models;

namespace backend.kapace.Models.Response;

public record V1RequestGetSearchByResponse(SearchContentUnit[] Units)
{
    public record SearchContentUnit(long ContentId, string Title, long ImageId);
}