using backend.kapace.BLL.Enums;

namespace backend.kapace.Models.Response;

public record V1GetMainPageContentResponse(V1GetMainPageContentResponse.V1GetMainPagePair[] PageContent)
{
    public record V1GetMainPagePair(
        MainPageType MainPageType,
        ContentInfo[] ContentsInfo
    );

    public record ContentInfo(
        QueryContent Content,
        string[] Genres);
}