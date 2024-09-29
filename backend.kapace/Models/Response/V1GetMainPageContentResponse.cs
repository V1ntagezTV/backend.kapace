using backend.kapace.BLL.Enums;

namespace backend.kapace.Models.Response;

public record V1GetMainPageContentResponse(V1GetMainPageContentResponse.V1GetMainPagePair[] Contents)
{
    public record V1GetMainPagePair(MainPageType MainPageType, QueryContent[] Content);
}